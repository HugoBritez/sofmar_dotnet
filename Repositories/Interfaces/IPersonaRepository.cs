using Api.Models.Entities;

namespace Api.Repositories.Interfaces
{
    public interface IPersonaRepository
    {
        Task<Persona> CreateAsync(Persona persona);
    }
}