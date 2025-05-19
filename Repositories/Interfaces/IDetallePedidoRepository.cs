using Api.Models.Entities;

namespace Api.Repositories.Interfaces
{
    public interface IDetallePedidoRepository
    {
        Task<DetallePedido> Crear(DetallePedido detalle);
    }
}