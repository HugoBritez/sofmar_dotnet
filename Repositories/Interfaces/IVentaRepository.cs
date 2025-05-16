using Api.Models.Entities;


namespace Api.Repositories.Interfaces
{
    public interface IVentaRepository
    {
        Task<Venta> CrearVenta(Venta venta);
    }
}