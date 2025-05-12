using Api.Auth.Models;
using MySqlConnector;
using Dapper;
using System.Text.Json;


namespace Api.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;
        

        public AuthService(IConfiguration configuration, IJwtService jwtService)
        {
            _configuration = configuration;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse> Login(string usuario, string password)
        {
            try
            {
                var connectionString = $"Server={_configuration["MYSQL_HOST"]};" +
                                 $"Database={_configuration["MYSQL_DB"]};" +
                                 $"User={usuario};" +
                                 $"Password={password};";

                Console.WriteLine($"Intentando conectar con: {connectionString}");

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                var query = @"
                    SELECT 
                        operadores.*,
                        orol.or_rol,
                        (SELECT JSON_ARRAYAGG(
                            JSON_OBJECT(
                                'menu_id', a.a_menu,
                                'menu_grupo', ms.m_grupo,
                                'menu_orden', ms.m_orden,
                                'menu_descripcion', ms.m_descripcion, 
                                'acceso', a.a_acceso
                            )
                        ) FROM acceso_menu_operador a
                        INNER JOIN menu_sistemas ms ON a.a_menu = ms.m_codigo 
                        WHERE a.a_operador = operadores.op_codigo) AS permisos_menu
                    FROM operadores 
                    LEFT JOIN operador_roles orol ON operadores.op_codigo = orol.or_operador
                    WHERE operadores.op_usuario = @usuario";

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(query, new { usuario });

                if (result == null)
                    throw new Exception("Usuario o contraseña incorrectos");

                var permisosMenu = JsonSerializer.Deserialize<List<PermisoMenu>>(result.permisos_menu);

                    var response = new LoginResponse
                    {
                        OpCodigo = result.op_codigo,
                        OpUsuario = result.op_usuario,
                        OrRol = result.or_rol,
                        PermisosMenu = permisosMenu
                    };

                    response.Token = _jwtService.GenerateToken(response);

                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de conexión: {ex.Message}");
                throw;
            }
        }
    }
}