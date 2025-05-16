using Api.Data;
using Api.Models.Entities;
using Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories.Implementations
{
    public class ListaPrecioRepository : IListaPrecioRepository
    {
        private readonly ApplicationDbContext _context;

        public ListaPrecioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ListaPrecio>> GetAll()
        {
            var listaPrecio = await _context.ListaPrecio
                               .Where(lp => lp.LpEstado == 1)
                               .ToListAsync();

            return listaPrecio;
        }
    }
}