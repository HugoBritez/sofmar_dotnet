using Api.Models.Dtos;
using Api.Models.Dtos.ArticuloLote;
using Api.Models.Entities;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;
using Api.Services.Mappers;


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

        public VentaService(IVentaRepository ventaRepository,
            IDetalleVentaRepository detalleVentaRepository,
            IDetalleBonificacionRepository detalleBonificacionRepository,
            IDetalleArticulosEditadoRepository detalleArticulosEditadoRepository,
            IDetalleVentaVencimientoRepository detalleVentaVencimientoRepository,
            IArticuloLoteRepository articuloLoteRepository)
        {
            _ventaRepository = ventaRepository;
            _detalleVentaRepository = detalleVentaRepository;
            _detalleBonificacionRepository = detalleBonificacionRepository;
            _detalleArticulosEditadoRepository = detalleArticulosEditadoRepository;
            _detalleVentaVencimientoRepository = detalleVentaVencimientoRepository;
            _articuloLoteRepository = articuloLoteRepository;
        }

        public async Task<Venta> CrearVenta(Venta venta, IEnumerable<DetalleVentaDTO> detalleVentaDTOs)
        {
            var ventaCreada = await _ventaRepository.CrearVenta(venta);

            foreach (var detalleDTO in detalleVentaDTOs)
            {
                var detalleVenta = detalleDTO.toDetalleVenta();

                if (detalleVenta != null)
                {
                    detalleVenta.Venta = ventaCreada.Codigo;
                    var detalleVentaCreado = await _detalleVentaRepository.CrearDetalleVenta(detalleVenta);
                    var idDetalleVenta = detalleVentaCreado.Codigo;

                    // Lote
                    if (detalleDTO.LoteId != 0)
                    {
                        var detalleVencimiento = detalleDTO.ToDetalleVencimiento((int)idDetalleVenta);
                        if (detalleVencimiento != null)
                        {
                            await _detalleVentaVencimientoRepository.CrearDetalleVencimiento(detalleVencimiento);
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

            return ventaCreada;
        }

    }
}