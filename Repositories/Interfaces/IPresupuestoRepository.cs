namespace Api.Repositories.Interfaces
{
    public interface IPresupuestosRepository
    {
        Task<string> ProcesarPresupuesto(int idPresupuesto);
    }
}