using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;


namespace Api.Repositories.Base
{
    public abstract class DapperRepositoryBase
    {
        protected readonly string _connectionString;

        public DapperRepositoryBase(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new ArgumentNullException(nameof(configuration), "La cadena de coneccion 'DefaultConnection' no fue encontrada");
        }

        protected MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}