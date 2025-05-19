namespace Api.Models.Dtos
{
    public class AsientoContableDTO
    {
        public uint Sucursal { get; set; }
        public uint Moneda { get; set; }
        public uint Operador { get; set; }
        public string Documento { get; set; } = string.Empty;
        public uint Numero { get; set; }
        public DateOnly Fecha { get; set; }
        public DateOnly FechaAsiento { get; set; }
        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }
        public decimal Cotizacion { get; set; }
        public uint Referencia { get; set; }
        public uint Origen { get; set; }
    }
}