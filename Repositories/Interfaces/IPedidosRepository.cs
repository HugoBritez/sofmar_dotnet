namespace Api.Repositories.Interfaces
{
    public interface IPedidosRepository
    {
        Task<string> ProcesarPedido(int idPedido);
    }
}