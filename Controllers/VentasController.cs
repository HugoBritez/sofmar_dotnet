using Microsoft.AspNetCore.Mvc;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Api.Models.Dtos;
using Api.Models.Entities;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ventas")]
    [Authorize]

    public class VentasController : ControllerBase
    {
        private readonly IVentaService _ventaService;

        public VentasController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        [HttpPost]
        public async Task<ActionResult<Venta>> CrearVenta(
            [FromBody] CrearVentaDTO crearVentaDTO
            )
        {
            if (crearVentaDTO == null)
            {
                return BadRequest("El objeto venta no puede ser nulo.");
            }

            var ventaCreada = await _ventaService.CrearVenta(crearVentaDTO.Venta, crearVentaDTO.DetalleVenta);
            return CreatedAtAction(nameof(CrearVenta), new { id = ventaCreada.Codigo }, ventaCreada);
        }
    }
}