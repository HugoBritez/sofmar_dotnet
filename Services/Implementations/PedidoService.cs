using Api.Models.Entities;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;
using Api.Audit.Services;
namespace Api.Services.Implementations
{
    public class PedidoService : IPedidosService
    {
        private readonly IPedidosRepository _pedidoRepository;
        private readonly IDetallePedidoRepository _detallePedidoRepository;
        private readonly IDetallePedidoFaltanteRepository _detallePedidoFaltanteRepository;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IArticuloLoteRepository _articuloLoteRepository;

        public PedidoService(
            IPedidosRepository pedidosRepository,
            IDetallePedidoRepository detallePedidoRepository,
            IDetallePedidoFaltanteRepository detallePedidoFaltanteRepository,
            IAuditoriaService auditoriaService,
            IArticuloLoteRepository articuloLoteRepository
        )
        {
            _pedidoRepository = pedidosRepository;
            _detallePedidoRepository = detallePedidoRepository;
            _detallePedidoFaltanteRepository = detallePedidoFaltanteRepository;
            _auditoriaService = auditoriaService;
            _articuloLoteRepository = articuloLoteRepository;
        }

        public async Task<Pedido> CrearPedido(Pedido pedido, IEnumerable<DetallePedido> detallePedido)
        {
            var pedidoCreado = await _pedidoRepository.CrearPedido(pedido);

            foreach (DetallePedido detalle in detallePedido)
            {
                DetallePedido detalleCreado = await _detallePedidoRepository.Crear(detalle);
                
                // Verificar la cantidad disponible en el lote
                var loteDetalle = detalle.CodigoLote;
                ArticuloLote? loteExistente = await _articuloLoteRepository.GetById(loteDetalle);
                
                if (loteExistente != null)
                {
                    // Caso 1: La cantidad solicitada es menor que la disponible en el lote
                    if (detalleCreado.Cantidad < loteExistente.AlCantidad)
                    {
                        // No hay faltantes, hay suficiente stock
                        Console.WriteLine($"Stock suficiente. Disponible: {loteExistente.AlCantidad}, Solicitado: {detalleCreado.Cantidad}");
                    }
                    // Caso 2: La cantidad solicitada es igual a la disponible en el lote
                    else if (detalleCreado.Cantidad == loteExistente.AlCantidad)
                    {
                        // No hay faltantes, pero se agota el stock completamente
                        Console.WriteLine($"Stock exacto. Disponible: {loteExistente.AlCantidad}, Solicitado: {detalleCreado.Cantidad}");
                    }
                    // Caso 3: La cantidad solicitada es mayor que la disponible en el lote
                    else
                    {
                        // Hay faltantes, registrar la cantidad que excede lo disponible
                        int cantidadFaltante = (int)(detalleCreado.Cantidad - loteExistente.AlCantidad);
                        
                        var detalleFaltante = new DetallePedidoFaltante
                        {
                            Codigo = 0,
                            DetallePedido = detalleCreado.Codigo,
                            Cantidad = cantidadFaltante,
                            Situacion = 0,
                            Observacion = $"Stock insuficiente. Disponible: {loteExistente.AlCantidad}, Solicitado: {detalleCreado.Cantidad}"
                        };

                        await _detallePedidoFaltanteRepository.Crear(detalleFaltante);
                    }
                }
                else
                {
                    // Este caso no debería ocurrir según los requisitos, pero lo dejamos como protección
                    Console.WriteLine("Error: Lote no encontrado, esto no debería ocurrir según los requisitos");
                }
            }

            return pedidoCreado;
        }
    }
}