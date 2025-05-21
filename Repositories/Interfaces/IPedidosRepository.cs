using Api.Models.Entities;
using Api.Models.ViewModels;

namespace Api.Repositories.Interfaces
{
    public interface IPedidosRepository
    {
        Task<IEnumerable<PedidoViewModel>> GetPedidos(
            string? fechaDesde,
            string? fechaHasta,
            string? nroPedido,
            int? articulo,
            string? clientes,
            string? vendedores,
            string? sucursales,
            string? estado,
            int? moneda,
            string? factura
        );

        Task<Pedido> GetById(uint codigo);
        Task<Pedido> CrearPedido(Pedido pedido);
        Task<string> ProcesarPedido(int idPedido);
        Task SaveChangesAsync();
    }
}