using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
    public class Marca
    {
    [Column("ma_codigo")]
    public uint MaCodigo { get; set; }

    [Column("ma_descripcion")]
    public string MaDescripcion { get; set; } = string.Empty;
}
}
