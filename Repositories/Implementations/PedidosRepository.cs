using Dapper;
using Api.Repositories.Base;
using Api.Repositories.Interfaces;
using Api.Data;

namespace Api.Repositories.Implementations
{
    public class PedidosRepository : DapperRepositoryBase, IPedidosRepository
    {
        private readonly ApplicationDbContext _context;
        public PedidosRepository(IConfiguration configuration, ApplicationDbContext context) : base(configuration)
        {
            _context = context;

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