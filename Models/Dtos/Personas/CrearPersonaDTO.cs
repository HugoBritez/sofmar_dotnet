using Api.Models.Entities;
namespace Api.Models.Dtos
{
    public class CrearPersonaDTO
    {
        public Persona? persona { get; set; }
        public Operador? Operador { get; set; }
        public Proveedor? Proveedor { get; set; }
        public Cliente? Cliente { get; set; }
        public IEnumerable<int> Tipo { get; set; } = [];// 0- cliente, 1- proveedor, 2- operador

    } 
}