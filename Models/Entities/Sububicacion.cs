using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
    public class Sububicacion
    {
        [Column("s_codigo")]
        public uint SCodigo { get; set; }

        [Column("s_descripcion")]
        public string SDescripcion { get; set; } = string.Empty;

        [Column("s_ubicacion")]
        public uint SUbicacion { get; set; }

        public virtual Ubicacion? Ubicacion { get; set; }
    }
}
