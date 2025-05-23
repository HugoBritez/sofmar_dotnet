using Api.Models.Entities;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;
using Api.Models.Dtos;
using Api.Models.ViewModels;
using Api.Models.Dtos.ArticuloLote;
using Api.Models.Dtos.Articulo;
namespace Api.Services.Implementations
{
    public class ControlIngresoService : IControlIngresoService
    {
        private readonly IComprasRepository _comprasRepository;
        private readonly IDetalleComprasRepository _detalleComprasRepository;
        private readonly ITransferenciasRepository _transferenciasRepository;
        private readonly ITransferenciasItemsRepository _transferenciasItemsRepository;
        private readonly ITransferenciasItemsVencimientoRepository _transferenciasItemsVencimientoRepository;
        private readonly IArticuloLoteRepository _articuloLoteRepository;
        private readonly IArticuloRepository _articuloRepository;

        public ControlIngresoService(
            IComprasRepository comprasRepository,
            IDetalleComprasRepository detalleComprasRepository,
            ITransferenciasRepository transferenciasRepository,
            ITransferenciasItemsRepository transferenciasItemsRepository,
            ITransferenciasItemsVencimientoRepository transferenciasItemsVencimientoRepository,
            IArticuloLoteRepository articuloLoteRepository,
            IArticuloRepository articuloRepository
        )
        {
            _comprasRepository = comprasRepository;
            _detalleComprasRepository = detalleComprasRepository;
            _transferenciasRepository = transferenciasRepository;
            _transferenciasItemsRepository = transferenciasItemsRepository;
            _transferenciasItemsVencimientoRepository = transferenciasItemsVencimientoRepository;
            _articuloLoteRepository = articuloLoteRepository;
            _articuloRepository = articuloRepository;
        }

        public async Task<bool> VerificarCompra(uint idCompra, uint userId)
        {
            var compra = await _comprasRepository.GetById(idCompra) ?? throw new InvalidOperationException("Compra no encontrada.");

            compra.Verificado = 1;
            compra.Verificador = (int)userId;

            await _comprasRepository.Update(compra);

            return true;
        }
        public async Task<bool> VerificarItem(uint detalle, decimal cantidad)
        {
            var item = await _detalleComprasRepository.GetById(detalle) ?? throw new InvalidOperationException("Item no encontrado");

            item.Cantidad = cantidad;

            await _detalleComprasRepository.Update(item);

            return true;
        }

        public async Task<bool> ConfirmarVerificacion(
            uint idCompra,
            string factura,
            uint deposito_inicial,
            uint deposito_destino,
            uint user_id,
            uint confirmador_id,
            IEnumerable<ItemConfirmarVerificacionDTO> items
        )
        {
            //1- Actualizamos el estado de la compra

            var compra = await _comprasRepository.GetById(idCompra) ?? throw new InvalidOperationException("Compra no encontrada");
            compra.Verificado = 2;
            compra.Confirmador = (int)user_id;

            await _comprasRepository.Update(compra);

            //2- Mapear la cabecera de la transferencia e insertarla

            var transferenciaAInsertar = new Transferencia
            {
                Id = 0,
                Fecha = DateTime.Now,
                Operador = user_id,
                Origen = deposito_inicial,
                Destino = deposito_destino,
                Comprobante = factura,
                Estado = 1,
                Motivo = "TRANSFERENCIA ENTRE DEPOSITOS POR VERIFICACION DE COMPRA",
                FechaOperacion = DateTime.Now,
                IdMaestro = 0,
                EstadoTransferencia = 1,
                UserAutorizador = 0,
                Talle = 0,
                Solicitud = 0
            };

            var transferencia = await _transferenciasRepository.Crear(transferenciaAInsertar);

            //3- Procesar cada item
            foreach (ItemConfirmarVerificacionDTO item in items)
            {
                // 3.1 Verificamos la existencia de lotes tanto en el origen como en el de destino
                var articuloAVerificar = await _articuloRepository.GetById((uint)item.IdArticulo);

                var loteOrigen = await _articuloLoteRepository.BuscarPorDeposito((uint)item.IdArticulo, articuloAVerificar.ArVencimiento, deposito_inicial, item.Lote);

                var loteFinal = await _articuloLoteRepository.BuscarPorDeposito((uint)item.IdArticulo, articuloAVerificar.ArVencimiento, deposito_destino, item.Lote);

                if (loteOrigen == null)
                {
                    throw new Exception($"No se encontro el lote inicial para el articulo {item.IdArticulo}");
                }

                // 3.2 procesamos las cantidades segun el caso

                if (loteFinal != null)
                {
                    decimal cantidad_final_origen = loteOrigen.AlCantidad - item.CantidadIngreso;
                    decimal cantifaf_final_destino = loteOrigen.AlCantidad + item.CantidadIngreso;

                    loteOrigen.AlCantidad = cantidad_final_origen;
                    await _articuloLoteRepository.Update(loteOrigen);

                    loteFinal.AlCantidad = cantifaf_final_destino;
                    await _articuloLoteRepository.Update(loteFinal);
                }
                else
                {
                    decimal cantidad_final_origen = loteOrigen.AlCantidad - item.CantidadIngreso;
                    decimal cantidad_final_destino = item.CantidadIngreso;

                    loteOrigen.AlCantidad = cantidad_final_origen;
                    await _articuloLoteRepository.Update(loteOrigen);

                    //mapeamos para guardar en articulos lotes el nuevo ingreso
                    var loteMapeado = new ArticuloLote
                    {
                        AlCodigo = 0,
                        AlArticulo = loteOrigen.AlArticulo,
                        AlDeposito = deposito_destino,
                        AlLote = loteOrigen.AlLote,
                        AlCantidad = cantidad_final_destino,
                        AlVencimiento = loteOrigen.AlVencimiento,
                        AlPreCompra = loteOrigen.AlPreCompra,
                        AlOrigen = loteOrigen.AlOrigen,
                        ALSerie = loteOrigen.ALSerie,
                        AlCodBarra = loteOrigen.AlCodBarra,
                        AlNroTalle = loteOrigen.AlNroTalle,
                        AlColor = loteOrigen.AlColor,
                        AlTalle = loteOrigen.AlTalle,
                        AlRegistro = loteOrigen.AlRegistro
                    };

                    await _articuloLoteRepository.Create(loteMapeado);

                }

                // 3.3 mapeamos y registramos las transferencias de los items y si aplica, los vencimientos

                var transferenciaItem = new TransferenciaItem
                {
                    Id = 0,
                    Transferencia = transferencia.Id,
                    Articulo = (uint)item.IdArticulo,
                    Cantidad = item.CantidadIngreso,
                    StockActualDestino = item.CantidadIngreso
                };

                var transferenciaItemCreado = await _transferenciasItemsRepository.Crear(transferenciaItem);

                if (articuloAVerificar.ArVencimiento == 1)
                {
                    var transferenciaVencimiento = new TransferenciaItemVencimiento
                    {
                        Id = 0,
                        IdItem = transferenciaItemCreado.Id,
                        Lote = item.Lote,
                        Fecha = loteOrigen.AlVencimiento,
                        LoteId = (int)loteOrigen.AlCodigo,
                        LoteIDD = (int)loteOrigen.AlCodigo,
                    };

                    await _transferenciasItemsVencimientoRepository.Crear(transferenciaVencimiento);
                }
            }
            return true;
        }
    }
}