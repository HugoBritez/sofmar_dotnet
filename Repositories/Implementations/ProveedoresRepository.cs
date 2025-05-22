using Api.Data;
using Api.Models.Entities;
using Api.Models.ViewModels;
using Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Api.Repositories.Implementations
{
    public class ProveedoresRepository : IProveedoresRepository
    {
        private readonly ApplicationDbContext _context;

        public ProveedoresRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProveedorViewModel>> GetProveedores(string? busqueda)
        {
            var query = _context.Proveedores
        .Include(p => p.Zona)
        .Select(p => new ProveedorViewModel
        {
            ProCodigo = p.Codigo,
            ProRazon = p.Razon,
            ProZona = p.Zona != null ? p.Zona.Descripcion : null
        });

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(p => p.ProRazon.Contains(busqueda));
            }

            return await query.Take(25).ToListAsync();
        }

        public async Task<Proveedor?> GetById(uint id)
        {
            var proveedor = await _context.Proveedores.FirstOrDefaultAsync(ca => ca.Codigo == id);
            return proveedor;
        }
    }
}
