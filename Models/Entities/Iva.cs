using System.ComponentModel.DataAnnotations.Schema;
namespace Api.Models.Entities
{
    public class Iva
{
    [Column("iva_codigo")]
    public uint IvaCodigo { get; set; }

    [Column("iva_descripcion")]
    public string IvaDescripcion { get; set; } = string.Empty;
}
}