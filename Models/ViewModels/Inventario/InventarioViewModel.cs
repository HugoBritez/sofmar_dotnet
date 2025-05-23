namespace Api.Models.ViewModels
{
    public class InventarioViewModel
    {
        public uint id { get; set; }
        public string fecha_inicio { get; set; } = "0001-01-01";
        public string hora_inicio { get; set; } = "00:00";
        public string fecha_cierre { get; set; } = "0001-01-01";
        public string hora_cierre { get; set; } = "00:00";
        public uint operador_id { get; set; }
        public string operador_nombre { get; set; } = string.Empty;
        public uint sucursal_id { get; set; }
        public string sucursal_nombre { get; set; } = string.Empty;
        public uint deposito_id { get; set; }
        public string deposito_nombre { get; set; } = string.Empty;
        public string nro_inventario { get; set; } = string.Empty;
        public int estado { get; set; }
        public int autorizado { get; set; }
    }
}