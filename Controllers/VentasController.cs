using Microsoft.AspNetCore.Mvc;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Api.Models.Dtos;
using Api.Models.Entities;
using Api.Models.ViewModels;
using Api.Repositories.Interfaces;
using Api.Repositories.Implementations;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ventas")]
    // [Authorize]

    public class VentasController : ControllerBase
    {
        private readonly IVentaService _ventaService;
        private readonly IVentaRepository _ventaRepository;

        public VentasController(IVentaService ventaService, IVentaRepository ventaRepository)
        {
            _ventaService = ventaService;
            _ventaRepository = ventaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<VentaViewModel>> ConsultaVentas(
            [FromQuery] string? fecha_desde,
            [FromQuery] string? fecha_hasta,
            [FromQuery] uint? sucursal,
            [FromQuery] uint? cliente,
            [FromQuery] uint? vendedor,
            [FromQuery] uint? articulo,
            [FromQuery] uint? moneda,
            [FromQuery] string? factura,
            [FromQuery] uint? venta,
            [FromQuery] uint? estadoVenta,
            [FromQuery] uint? remisiones,
            [FromQuery] bool? listaFacturasSinCDC,
            [FromQuery] int? page = 1,
            [FromQuery] int itemsPorPagina = 50
        )
        {
            var ventas = await _ventaRepository.ConsultaVentas(
                fecha_desde, fecha_hasta, sucursal, cliente, vendedor, articulo, moneda, factura, venta, estadoVenta, remisiones, listaFacturasSinCDC, page, itemsPorPagina
            );

            return Ok(ventas);
        }


        [HttpGet("detalles")]
        public async Task<ActionResult<DetalleVentaViewModel>> ConsultaDetalles(
            [FromQuery] uint ventaId
        )
        {
            var detalleVenta = await _ventaRepository.ConsultaDetalles(ventaId);
            return Ok(detalleVenta);
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