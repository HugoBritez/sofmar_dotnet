using Api.Models.Dtos;
using Api.Models.Entities;
using Api.Models.ViewModels;

namespace Api.Repositories.Interfaces{
    public interface IClienteRepository
    {
        Task<IEnumerable<ClienteViewModel>> GetClientes(string? busqueda, uint? id, uint? interno, uint? vendedor, int? estado);

        Task<Cliente> CrearCliente(Cliente data);
    }
}
