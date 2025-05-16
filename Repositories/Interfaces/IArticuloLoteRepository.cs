using Api.Models.Entities;
using Api.Models.Dtos.ArticuloLote;
namespace Api.Repositories.Interfaces
{
    public interface IArticuloLoteRepository
    {
        Task<IEnumerable<ArticuloLote?>> GetByArticulo(uint articuloId);
        Task<ArticuloLote?> GetById(uint id);

        Task<ArticuloLote> Create(ArticuloLote articuloLote);

        Task<ArticuloLote?> UpdatePartial(uint id, ArticuloLotePatchDTO articuloLotePatchDTO);
    }
}
