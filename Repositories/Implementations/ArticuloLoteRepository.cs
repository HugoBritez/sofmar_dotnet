using Api.Data;
using Api.Models.Dtos.ArticuloLote;
using Api.Models.Entities;
using Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Api.Repositories.Implementations
{
    public class ArticuloLoteRepository : IArticuloLoteRepository
    {
        private readonly ApplicationDbContext _context;

        public ArticuloLoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ArticuloLote?>> GetByArticulo(uint articuloId)
        {
            return await _context.ArticuloLotes
                .Where(al => al.AlArticulo == articuloId)
                .ToListAsync();
        }

        public async Task<ArticuloLote?> GetById(uint id)
        {
            return await _context.ArticuloLotes
                .Where(al => al.AlCodigo == id)
                .FirstOrDefaultAsync();
        }

        public async Task<ArticuloLote> Create(ArticuloLote articuloLote)
        {
            await _context.ArticuloLotes.AddAsync(articuloLote);
            await _context.SaveChangesAsync();
            return articuloLote;
        }


        public async Task<ArticuloLote?> UpdatePartial(uint id, ArticuloLotePatchDTO articuloLotePatchDTO)
        {
            var existingArticuloLote = await GetById(id);
            if (existingArticuloLote is null)
            {
                return null;
            }

            if (articuloLotePatchDTO.AlArticulo.HasValue)
            {
                existingArticuloLote.AlArticulo = articuloLotePatchDTO.AlArticulo.Value;
            }

            if (articuloLotePatchDTO.AlDeposito.HasValue)
            {
                existingArticuloLote.AlDeposito = articuloLotePatchDTO.AlDeposito.Value;
            }

            if (articuloLotePatchDTO.AlLote != null)
            {
                existingArticuloLote.AlLote = articuloLotePatchDTO.AlLote;
            }

            if (articuloLotePatchDTO.AlCantidad.HasValue)
            {
                existingArticuloLote.AlCantidad = articuloLotePatchDTO.AlCantidad.Value;
            }

            if (articuloLotePatchDTO.AlVencimiento.HasValue)
            {
                existingArticuloLote.AlVencimiento = articuloLotePatchDTO.AlVencimiento.Value;
            }

            if (articuloLotePatchDTO.AlPreCompra.HasValue)
            {
                existingArticuloLote.AlPreCompra = articuloLotePatchDTO.AlPreCompra.Value;
            }

            if (articuloLotePatchDTO.AlOrigen.HasValue)
            {
                existingArticuloLote.AlOrigen = articuloLotePatchDTO.AlOrigen.Value;
            }

            if (articuloLotePatchDTO.ALSerie != null)
            {
                existingArticuloLote.ALSerie = articuloLotePatchDTO.ALSerie;
            }

            if (articuloLotePatchDTO.AlCodBarra != null)
            {
                existingArticuloLote.AlCodBarra = articuloLotePatchDTO.AlCodBarra;
            }

            if (articuloLotePatchDTO.AlNroTalle != null)
            {
                existingArticuloLote.AlNroTalle = articuloLotePatchDTO.AlNroTalle;
            }

            if (articuloLotePatchDTO.AlColor.HasValue)
            {
                existingArticuloLote.AlColor = articuloLotePatchDTO.AlColor.Value;
            }

            if (articuloLotePatchDTO.AlTalle.HasValue)
            {
                existingArticuloLote.AlTalle = articuloLotePatchDTO.AlTalle.Value;
            }

            if (articuloLotePatchDTO.AlRegistro != null)
            {
                existingArticuloLote.AlRegistro = articuloLotePatchDTO.AlRegistro;
            }

            await _context.SaveChangesAsync();
            return existingArticuloLote;
        }

    }
}
