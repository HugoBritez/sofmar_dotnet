using Api.Models.Dtos;
using Api.Models.Dtos.ArticuloLote;
using Api.Models.Entities;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;
using Api.Services.Mappers;
using Api.Audit.Services;
using Api.Audit.Models;
namespace Api.Services.Implementations
{
    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IDetalleVentaRepository _detalleVentaRepository;
        private readonly IDetalleBonificacionRepository _detalleBonificacionRepository;
        private readonly IDetalleArticulosEditadoRepository _detalleArticulosEditadoRepository;
        private readonly IDetalleVentaVencimientoRepository _detalleVentaVencimientoRepository;
        private readonly IArticuloLoteRepository _articuloLoteRepository;
        private readonly IContabilidadService _contabilidadService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ICotizacionRepository _cotizacionRepository;
        private readonly IPedidosRepository _pedidosRepository;
        private readonly IDetallePedidoRepository _detallePedidosRepository;

        public VentaService(IVentaRepository ventaRepository,
            IDetalleVentaRepository detalleVentaRepository,
            IDetalleBonificacionRepository detalleBonificacionRepository,
            IDetalleArticulosEditadoRepository detalleArticulosEditadoRepository,
            IDetalleVentaVencimientoRepository detalleVentaVencimientoRepository,
            IArticuloLoteRepository articuloLoteRepository,
            IContabilidadService contabilidadService,
            IAuditoriaService auditoriaService,
            IPedidosRepository pedidosRepository,
            IDetallePedidoRepository detallePedidoRepository,
            ICotizacionRepository cotizacionRepository
        )
        {
            _ventaRepository = ventaRepository;
            _detalleVentaRepository = detalleVentaRepository;
            _detalleBonificacionRepository = detalleBonificacionRepository;
            _detalleArticulosEditadoRepository = detalleArticulosEditadoRepository;
            _detalleVentaVencimientoRepository = detalleVentaVencimientoRepository;
            _articuloLoteRepository = articuloLoteRepository;
            _contabilidadService = contabilidadService;
            _auditoriaService = auditoriaService;
            _pedidosRepository = pedidosRepository;
            _detallePedidosRepository = detallePedidoRepository;
            _cotizacionRepository = cotizacionRepository;
        }
        public async Task<Venta> CrearVenta(VentaDTO venta, IEnumerable<DetalleVentaDTO> detalleVentaDTOs)
        {
            var ventaCreada = await _ventaRepository.CrearVenta(venta);
            decimal totalExentas = 0;
            decimal totalCinco = 0;
            decimal totalDiez = 0;

            decimal costoTotalExentas = 0;
            decimal costoTotalCinco = 0;
            decimal costoTotalDiez = 0;


            foreach (var detalleDTO in detalleVentaDTOs)
            {
                var detalleVenta = detalleDTO.toDetalleVenta();

                if (detalleVenta != null)
                {
                    totalExentas += detalleVenta.Exentas;
                    totalCinco += detalleVenta.Cinco;
                    totalDiez += detalleVenta.Diez;

                    if (detalleVenta.Exentas > 0)
                    {
                        costoTotalExentas += detalleVenta.Costo;
                    }
                    else if (detalleVenta.Cinco > 0)
                    {
                        costoTotalCinco += detalleVenta.Costo;
                    }
                    else if (detalleVenta.Diez > 0)
                    {
                        costoTotalDiez += detalleVenta.Costo;
                    }


                    detalleVenta.Venta = ventaCreada.Codigo;
                    var detalleVentaCreado = await _detalleVentaRepository.CrearDetalleVenta(detalleVenta);
                    var idDetalleVenta = detalleVentaCreado.Codigo;

                    Console.WriteLine("LoteId del detalleDTO: " + detalleDTO.LoteId);

                    // Lote
                    if (detalleDTO.LoteId != 0)
                    {
                        Console.WriteLine("LoteId del detalleDTO no es cero, procesando lote..." + detalleDTO.LoteId + " para DetalleVenta " + idDetalleVenta); ;
                        var detalleVencimiento = detalleDTO.ToDetalleVencimiento((int)idDetalleVenta);
                        Console.WriteLine("DetalleVencimiento creado: " + (detalleVencimiento != null));
                        if (detalleVencimiento != null)
                        {
                            try
                            {
                                await _detalleVentaVencimientoRepository.CrearDetalleVencimiento(detalleVencimiento);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Error al crear vencimiento para DetalleVenta {idDetalleVenta}, Lote {detalleDTO.LoteId}: {ex.Message}");
                            }
                        }

                        var articuloLoteActual = await _articuloLoteRepository.GetById((uint)detalleDTO.LoteId);
                        if (articuloLoteActual != null)
                        {
                            var articuloLotePatch = new ArticuloLotePatchDTO
                            {
                                AlCantidad = articuloLoteActual.AlCantidad - detalleDTO.DeveCantidad,
                            };

                            await _articuloLoteRepository.UpdatePartial(articuloLoteActual.AlCodigo, articuloLotePatch);
                        }
                    }

                    // Bonificación
                    if (detalleDTO.DeveBonificacion != 0)
                    {
                        var detalleBonificacion = detalleDTO.ToBonificacion((int)idDetalleVenta);
                        if (detalleBonificacion != null)
                        {
                            await _detalleBonificacionRepository.CrearDetalleBonificacion(detalleBonificacion);
                        }
                    }

                    // Artículo editado
                    if (detalleDTO.ArticuloEditado)
                    {
                        var detalleArticuloEditado = detalleDTO.ToDetalleArticulosEditado((int)idDetalleVenta);
                        if (detalleArticuloEditado != null)
                        {
                            await _detalleArticulosEditadoRepository.CrearDetalleArticulosEditado(detalleArticuloEditado);
                        }
                    }
                }
            }

            var imprimirLegal = !string.IsNullOrWhiteSpace(venta.Factura) ? 1u : 0u;
            var cotizacionDolar = await _cotizacionRepository.GetCotizacionDolarHoy();

            var asientoContable = new GuardarAsientoContableDTO
            {
                Automatico = true,
                TipoVenta = ventaCreada.Credito,
                Moneda = ventaCreada.Moneda,
                Sucursal = ventaCreada.Sucursal,
                Factura = ventaCreada.Factura,
                Operador = ventaCreada.Vendedor,
                Fecha = ventaCreada.Fecha,
                TotalAPagar = ventaCreada.Total,
                NumeroAsiento = venta.Codigo,
                Cotizacion = cotizacionDolar != null ? cotizacionDolar.Monto : 7300,
                TotalExentas = totalExentas,
                TotalCinco = totalCinco,
                TotalDiez = totalDiez,
                ImprimirLegal = imprimirLegal,
                CajaDefinicion = venta.CajaDefinicion,
                Referencia = ventaCreada.Codigo
            };

            var asientoContableCosto = new GuardarCostoAsientoContableDTO
            {
                Automatico = true,
                Moneda = ventaCreada.Moneda,
                Sucursal = ventaCreada.Sucursal,
                Factura = ventaCreada.Factura,
                Operador = ventaCreada.Vendedor,
                Fecha = ventaCreada.Fecha,
                CostoTotalCinco = costoTotalCinco,
                CostoTotalDiez = costoTotalDiez,
                CostoTotalExentas = costoTotalExentas,
                Cotizacion = cotizacionDolar != null ? cotizacionDolar.Monto : 7300,
                MonedaDolar = ventaCreada.Moneda,
                ImprimirLegal = imprimirLegal,
                Referencia = ventaCreada.Codigo
            };

            await _contabilidadService.GuardarAsientoContable(asientoContable);
            await _contabilidadService.GuardarCostoAsientoContable(asientoContableCosto);

            await _auditoriaService.RegistrarAuditoria(5, 1, (int)ventaCreada.Codigo, "Usuario Web", (int)ventaCreada.Vendedor, "Venta creada desde el sistema web");
            return ventaCreada;
        }


        

    }
}