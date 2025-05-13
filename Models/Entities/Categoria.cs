using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
    public class Categoria
    {
    [Column("ca_codigo")]
    public uint CaCodigo { get; set; }

    [Column("ca_descripcion")]
    public string CaDescripcion { get; set; } = string.Empty;
}
}
