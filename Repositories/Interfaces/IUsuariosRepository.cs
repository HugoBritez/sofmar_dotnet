using Api.Models.Dtos;

namespace Api.Repositories.Interfaces{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<UsuarioViewModel>> GetUsuarios(string? busqueda, uint? id);
    }
}
