using Api.Data;
using Api.Models.Entities;
using Api.Repositories.Base;
using Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories.Implementations
{
    public class DetallePedidoRepository : DapperRepositoryBase, IDetallePedidoRepository
    {
        private readonly ApplicationDbContext _context;

        public DetallePedidoRepository(IConfiguration configuration, ApplicationDbContext context) : base(configuration)
        {
            _context = context;
        }

        public async Task<DetallePedido> Crear(DetallePedido detalle)
        {
            var detallePedidoCreado = await _context.DetallePedido.AddAsync(detalle);
            await _context.SaveChangesAsync();

            return detallePedidoCreado.Entity;
        }
    }
}