using System.Text.Json.Serialization;

namespace Api.Auth.Models
{
    public class LoginResponse
    {
        [JsonPropertyName("op_codigo")]
        public uint OpCodigo { get; set; }

        [JsonPropertyName("op_usuario")]
        public string OpUsuario { get; set; } = string.Empty;

        [JsonPropertyName("or_rol")]
        public uint OrRol { get; set; }

        [JsonPropertyName("permisos_menu")]
        public List<PermisoMenu> PermisosMenu { get; set; } = [];

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }

    public class PermisoMenu
    {
        [JsonPropertyName("menu_id")]
        public uint MenuId { get; set; }

        [JsonPropertyName("menu_grupo")]
        public uint MenuGrupo { get; set; }

        [JsonPropertyName("menu_orden")]
        public uint MenuOrden { get; set; }

        [JsonPropertyName("menu_descripcion")]
        public string MenuDescripcion { get; set; } = string.Empty;

        [JsonPropertyName("acceso")]
        public uint Acceso { get; set; }
    }
}