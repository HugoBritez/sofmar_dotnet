using Dapper;
using Api.Repositories.Base;
using Api.Repositories.Interfaces;
using Api.Data;
using Api.Models.Entities;

namespace Api.Repositories.Implementations
{
    public class PedidosRepository : DapperRepositoryBase, IPedidosRepository
    {
        private readonly ApplicationDbContext _context;
        public PedidosRepository(IConfiguration configuration, ApplicationDbContext context) : base(configuration)
        {
            _context = context;

        }

        public async Task<Pedido> CrearPedido(Pedido pedido)
        {
            var pedidoCreado = await _context.Pedido.AddAsync(pedido);
            await _context.SaveChangesAsync();

            return pedidoCreado.Entity; 
        }

        public async Task<string> ProcesarPedido(int idPedido)
        {
            var connection = GetConnection();


            var query = @"
                UPDATE pedidos
                SET p_estado = 2
                WHERE p_id = @idPedido
                ";

            var parameter = new DynamicParameters();
            parameter.Add("idPedido", idPedido);
            var result = await connection.ExecuteAsync(query, parameter);
            if (result > 0)
            {
                return "Pedido procesado correctamente";
            }
            else
            {
                return "Error al procesar el pedido";
            }
        }
    }
}       