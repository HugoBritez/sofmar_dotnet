using Dapper;
using Api.Repositories.Base;
using Api.Repositories.Interfaces;
using Api.Data;

namespace Api.Repositories.Implementations
{
    public class PresupuestosRepository : DapperRepositoryBase, IPresupuestosRepository
    {
        private readonly ApplicationDbContext _context;
        public PresupuestosRepository(IConfiguration configuration, ApplicationDbContext context) : base(configuration)
        {
            _context = context;

        }

        public async Task<string> ProcesarPresupuesto(int idPresupuesto)
        {
            var connection = GetConnection();
            var query = @"
                UPDATE presupuesto
                SET pre_confirmado = 1
                WHERE pre_codigo = @idPresupuesto
                ";

            var parameter = new DynamicParameters();
            parameter.Add("idPresupuesto", idPresupuesto);
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