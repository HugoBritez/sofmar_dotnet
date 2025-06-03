using Api.Models.Dtos;
using Api.Models.Entities;

namespace Api.Repositories.Interfaces{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<UsuarioViewModel>> GetUsuarios(string? busqueda, uint? id);

        Task<Operador> CrearOperador(Operador data);
    }
}
