using Api.Models.Dtos;
using Api.Models.Entities;

namespace Api.Services.Interfaces
{
    public interface IVentaService
    {
        Task<Venta> CrearVenta(Venta venta, DetalleVentaDTO detalleVenta);
    }
}