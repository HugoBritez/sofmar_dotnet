using Api.Models.Entities;

namespace Api.Repositories.Interfaces
{
    public interface IListaPrecioRepository
    {
        Task<IEnumerable<ListaPrecio>> GetAll();
    }
}