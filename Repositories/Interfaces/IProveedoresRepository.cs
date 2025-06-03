using Api.Models.Entities;
using Api.Models.ViewModels;

namespace Api.Repositories.Interfaces{
    public interface IProveedoresRepository
    {
        Task<IEnumerable<ProveedorViewModel>> GetProveedores(string? busqueda);
        Task<Proveedor?> GetById(uint id);

        Task<Proveedor> CrearProveedor(Proveedor data);
    }
}
