using Api.Models.Dtos.Sucursal;
using Api.Repositories.Interfaces;
using Dapper;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories.Implementations
{
    public class SucursalRepository : ISucursalRepository
    {
        public required string _connectionString { get; init;}
        public SucursalRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new ArgumentNullException(nameof(configuration), "La cadena de conexión 'DefaultConnection' no fue encontrada.");
        }

        public async Task<IEnumerable<SucursalDTO>> GetSucursales(
            uint? operador = null,
            uint? matriz = null
        )
        {
            var where = new List<string>();
            var parameters = new DynamicParameters();

            if(operador.HasValue){
                where.Add("op.op_codigo = @OperadorId");
                parameters.Add("@OperadorId", operador.Value);
            }

            if(matriz.HasValue){
                where.Add("suc.matriz = @MatrizId");
                parameters.Add("@MatrizId", matriz.Value);
            }

            var query = @"
                SELECT suc.id, suc.descripcion, suc.direccion, suc.nombre_emp, suc.ruc_emp, suc.matriz
                FROM sucursales suc
                INNER JOIN operadores op ON op.op_sucursal = suc.id
                WHERE 1 = 1";

            if(where.Count != 0){
                query += " AND " + string.Join(" AND ", where);
            }

            query += " GROUP BY suc.id, suc.descripcion, suc.direccion, suc.nombre_emp, suc.ruc_emp, suc.matriz";

            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryAsync<SucursalDTO>(query, parameters);
        }
    }
}
