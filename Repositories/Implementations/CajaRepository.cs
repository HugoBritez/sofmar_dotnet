using Api.Data;
using Api.Models.Dtos;
using Api.Models.ViewModels;
using Api.Repositories.Base;
using Api.Repositories.Interfaces;
using Dapper;

namespace Api.Repositories.Implementations
{
    public class CajaRepository : DapperRepositoryBase, ICajaRepository
    {
        private readonly ApplicationDbContext _context;

        public CajaRepository(IConfiguration configuration, ApplicationDbContext context) : base(configuration)
        {
            _context = context;
        }

        public async Task<IEnumerable<CajaViewModel>> VerificarCajaAbierta(uint operador, uint moneda)
        {
            var connection = GetConnection();
            var parameters = new DynamicParameters();
            
            var query = @"
              SELECT
                IFNULL(ca.ca_codigo, 0) as Codigo,
                ca.ca_definicion as Definicion
              FROM
                caja ca
              WHERE
                ca.ca_operador = @operador AND
                ca.ca_situacion = 1 AND
                ca.ca_moneda = @moneda AND
                ca.ca_tipo_caja = 1 AND
                ca.ca_prioridad = 1
              ORDER BY ca.ca_fecha DESC
              LIMIT 1 
            ";

            parameters.Add("operador", operador);
            parameters.Add("moneda", moneda);
            return await connection.QueryAsync<CajaViewModel>(query, parameters);
        }
    }
}