using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
    public class Ubicacion
    {
        [Column("ub_codigo")]
        public uint UbCodigo { get; set; }

        [Column("ub_descripcion")]
        public string UbDescripcion { get; set; } = string.Empty;

        [Column("ub_estado")]
        public byte UbEstado { get; set; } = 1;

    }
}