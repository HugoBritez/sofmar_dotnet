using Microsoft.AspNetCore.Mvc;
using Api.Repositories.Interfaces;
using Api.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/lista-precios")]
    [Authorize]
    public class ListaPreciosController : ControllerBase
    {
        private readonly IListaPrecioRepository _listaPrecioRepository;

        public ListaPreciosController(IListaPrecioRepository listaPrecioRepository)
        {
            _listaPrecioRepository = listaPrecioRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListaPrecio>>> GetListaPrecios()
        {
            var listaPrecios = await _listaPrecioRepository.GetAll();
            return Ok(listaPrecios);
        }

    }
}
