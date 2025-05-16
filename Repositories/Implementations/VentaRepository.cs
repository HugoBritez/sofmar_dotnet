using Api.Models.Entities;
using Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Api.Data;

namespace Api.Repositories.Implementations
{
    public class VentaRepository : IVentaRepository
    {
        private readonly string _connectionString;

        private readonly ApplicationDbContext _context;
        public VentaRepository(IConfiguration configuration, ApplicationDbContext context)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new ArgumentNullException(nameof(configuration), "La cadena de coneccion 'DefaultConnection' no fue encontrada");
        }

        public async Task<Venta> CrearVenta(Venta venta)
        {
            var ventaCreada = await _context.Venta.AddAsync(venta);
            await _context.SaveChangesAsync();
            return ventaCreada.Entity;
        }


    }
}