using Api.Audit.Models;
using Dapper;
using MySql.Data.MySqlClient;


namespace Api.Audit.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly string _connectionString;

        public AuditoriaService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Auditoria> RegistrarAuditoria(
            int entidad,
            int accion,
            int idReferencia,
            string usuario,
            int vendedor,
            string obs
        )
        {
            Console.WriteLine($"Registrando auditoria para usuario: {usuario}");
            Console.WriteLine($"Entidad: {entidad}, Accion: {accion}, IdReferencia: {idReferencia}, Usuario: {usuario}, Vendedor: {vendedor}, Obs: {obs}");
            var query = @"
              INSERT INTO auditoria (
                entidad,
                accion,
                fecha,
                idReferencia,
                usuario,
                vendedor,
                obs
              )
              VALUES (
                @entidad,
                @accion,
                NOW(),
                @idReferencia,
                @usuario,
                @vendedor,
                @obs
              )
            ";

            using var connection = new MySqlConnection(_connectionString);
            await connection.ExecuteAsync(query, new
              {
                entidad,
                accion,
                idReferencia,
                usuario,
                vendedor,
                obs
              }
            );

            return new Auditoria
            {
                Id = idReferencia,
                Entidad = entidad,
                Accion = accion,
                Fecha = DateTime.Now,
                IdReferencia = idReferencia,
                Usuario = usuario,
                Vendedor = vendedor,
                Obs = obs
            };
        }
    }
}
