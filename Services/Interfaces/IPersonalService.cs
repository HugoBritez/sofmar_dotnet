using Api.Models.Dtos;

namespace Api.Services.Interfaces
{
    public interface IPersonalService
    {
        Task<bool?> CrearPersona(CrearPersonaDTO data);
    }
}