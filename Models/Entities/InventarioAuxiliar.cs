using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.Entities
{
    [Table("inventario_auxiliar")]
    public class InventarioAuxiliar
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }
        [Column("fecha")]
        public string FechaInicio { get; set; } = string.Empty;
        [Column("hora")]
        public string HoraInicio { get; set; } = string.Empty;
        [Column("operador")]
        public uint Operador { get; set; }
        [Column("sucursal")]
        public uint Sucursal { get; set; }
        [Column("deposito")]
        public uint Deposito { get; set; }
        [Column("estado")]
        public int Estado { get; set; }
        [Column("obs")]
        public string Observacion { get; set; } = string.Empty;
        [Column("nro_inventario")]
        public string NroInventario { get; set; } = string.Empty;
        [Column("autorizado")]
        public int Autorizado { get; set; }
        [Column("fecha_cierre")]
        public string FechaCierre { get; set; } = string.Empty;
        [Column("hora_cierre")]
        public string HoraCierre { get; set; } = string.Empty;
    }
}