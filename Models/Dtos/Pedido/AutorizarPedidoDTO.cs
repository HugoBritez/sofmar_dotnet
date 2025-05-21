namespace Api.Models.Dtos
{
    public class AutorizarPedidoDTO
    {
        public uint idPedido { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }
}