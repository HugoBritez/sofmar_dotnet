using Api.Models.Dtos.Articulo;

namespace Api.Repositories.Interfaces
{
    public interface IArticuloRepository
    {
        Task<IEnumerable<ArticuloBusquedaDTO>> BuscarArticulos(
            uint? articuloId = null,
            string? busqueda = null,
            string? codigoBarra = null,
            uint moneda = 1,
            bool? stock = null,
            uint? deposito = null,
            uint? marca = null,
            uint? categoria = null,
            uint? ubicacion = null,
            uint? proveedor = null,
            string? codInterno = null,
            string? lote = null,
            bool? negativo = null
        );

        Task<ArticuloConsultaResponse> ConsultarArticulos(
        string? busqueda = null,
        string? deposito = null,
        string? stock = null,
        string? marca = null,
        string? categoria = null,
        string? subcategoria = null,
        string? proveedor = null,
        string? ubicacion = null,
        bool? servicio = null,
        uint? moneda = null,
        string? unidadMedida = null,
        int pagina = 1,
        int limite = 50,
        string? tipoValorizacionCosto = null);

        Task<IEnumerable<ArticuloCategoriaResponse>> ArticulosPorCategoria();

        Task<IEnumerable<ArticuloMarcaResponse>> ArticulosPorMarca();

        Task<IEnumerable<ArticuloSeccionResponse>> ArticulosPorSeccion();

        Task<IEnumerable<ArticuloEnPedidoResponse>> ArticulosEnPedido( int articulo_id, int id_lote);

    }
}
