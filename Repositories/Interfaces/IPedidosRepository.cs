using Api.Models.Entities;

namespace Api.Repositories.Interfaces
{
    public interface IPedidosRepository
    {
        Task<Pedido> CrearPedido(Pedido pedido);
        Task<string> ProcesarPedido(int idPedido);
    }
}