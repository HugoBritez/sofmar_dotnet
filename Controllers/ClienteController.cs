using Microsoft.AspNetCore.Mvc;
using Api.Repositories.Interfaces;
using Api.Models.Dtos;
using Api.Models.ViewModels;


namespace Api.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    // [Authorize]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteController(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteViewModel>>> GetClientes(
           [FromQuery] string? busqueda,
           [FromQuery] uint? id,
           [FromQuery] uint? interno,
           [FromQuery] uint? vendedor,
           [FromQuery] int? estado
        )
        {
            var clientes = await _clienteRepository.GetClientes(busqueda,id, interno, vendedor, estado);
            return Ok(clientes);
        }

    }
}
