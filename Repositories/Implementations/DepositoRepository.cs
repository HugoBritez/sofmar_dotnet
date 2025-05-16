using Api.Models.Dtos.Deposito;
using Api.Repositories.Interfaces;
using Dapper;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories.Implementations
{
    public class DepositoRepository : IDepositosRepository
    {
        public required string _connectionString { get; init;}

        public DepositoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new ArgumentNullException(nameof(configuration), "La cadena de conexión 'DefaultConnection' no fue encontrada.");

        }

        public async Task<IEnumerable<DepositoDTO>> GetDepositos(
            uint? sucursal = null,
            uint? usuario = null,
            string? descripcion = null
        )
        {
            var where = new List<string>();
            var parameters = new DynamicParameters();

            if(sucursal.HasValue)
            {
                where.Add("dep.dep_codigo = @SucursalId");
                parameters.Add("@SucursalId", sucursal.Value);
            }

            if(usuario.HasValue)
            {
                using var userConnection = new MySqlConnection(_connectionString);
                var usuario_sucursal = await userConnection.QueryFirstOrDefaultAsync<uint?>(
                    "SELECT op_sucursal FROM operadores WHERE op_codigo = @UsuarioId",
                    new { UsuarioId = usuario.Value }
                );

                if(usuario_sucursal.HasValue)
                {
                    where.Add("dep.dep_codigo = @UsuarioSucursalId");
                    parameters.Add("@UsuarioSucursalId", usuario_sucursal.Value);
                }
            }

            if(!string.IsNullOrEmpty(descripcion))
            {
                where.Add("dep.dep_descripcion LIKE @Descripcion");
                parameters.Add("@Descripcion", $"%{descripcion}%");
            }

            var query = @"
                SELECT dep.dep_codigo, dep.dep_descripcion, dep_principal
                FROM depositos dep
                WHERE 1 = 1";

            if (where.Count != 0)
            {
                query += " AND " + string.Join(" AND ", where);
            }
           
            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryAsync<DepositoDTO>(query, parameters);
        }
    }


}


