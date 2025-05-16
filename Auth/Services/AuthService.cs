using Api.Auth.Models;
using MySqlConnector;
using Dapper;
using System.Text.Json;
using Api.Audit.Services;

namespace Api.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;

        private readonly IAuditoriaService _auditoriaService;


        public AuthService(IConfiguration configuration, IJwtService jwtService, IAuditoriaService auditoriaService)
        {
            _configuration = configuration;
            _jwtService = jwtService;
            _auditoriaService = auditoriaService;
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

                    var usuarioResponse = new UsuarioResponse
                    {
                        OpCodigo = Convert.ToUInt32(result.op_codigo),
                        OpNombre = result.op_nombre,
                        OpSucursal = Convert.ToUInt32(result.op_sucursal),
                        OpAutorizar = Convert.ToInt32(result.op_autorizar),
                        OpVerUtilidad = Convert.ToInt32(result.op_ver_utilidad),
                        OpVerProveedor = Convert.ToInt32(result.op_ver_proveedor),
                        OpAplicarDescuento = Convert.ToInt32(result.op_aplicar_descuento),
                        OpMovimiento = Convert.ToInt32(result.op_movimiento),
                        OrRol = Convert.ToInt32(result.or_rol),
                        PermisosMenu = permisosMenu
                    };

                    var response = new LoginResponse
                    {
                        Token = _jwtService.GenerateToken(usuarioResponse),
                        Usuario = new List<UsuarioResponse> { usuarioResponse }
                    };

                    await _auditoriaService.RegistrarAuditoria(
                        10,
                        4,
                        (int)response.Usuario[0].OpCodigo,
                        usuario,
                        (int)response.Usuario[0].OpCodigo,
                        "Inicio de sesión desde el sistema web"
                    );

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