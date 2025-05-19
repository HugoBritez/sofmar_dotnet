using Api.Models.Entities;

namespace Api.Services.Interfaces
{
    public interface IPedidosService
    {
        Task<Pedido> CrearPedido(Pedido pedido, IEnumerable<DetallePedido> detallePedido);
    }
}