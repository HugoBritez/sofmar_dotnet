using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
    public class SubCategoria
    {
        [Column("sc_codigo")]
        public uint ScCodigo { get; set; }

        [Column("sc_descripcion")]
        public string ScDescripcion { get; set; } = string.Empty;

        [Column("sc_categoria")]
        public uint ScCategoria { get; set; }

        public virtual Categoria? Categoria { get; set; }
    }
}
