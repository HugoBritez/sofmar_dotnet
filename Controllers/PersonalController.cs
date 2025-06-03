using Microsoft.AspNetCore.Mvc;
using Api.Repositories.Interfaces;
using Api.Models.Dtos;
using Api.Services.Interfaces;


namespace Api.Controllers
{
    [ApiController]
    [Route("api/personal")]
    // [Authorize]
    public class PersonalController : ControllerBase
    {
        private readonly IPersonalService _personalService;

        public PersonalController(IPersonalService personalService)
        {
            _personalService = personalService;
        }

        [HttpPost]
        public async Task<ActionResult<bool>> CrearPersonal([FromBody] CrearPersonaDTO datos)
        {
            var res = await _personalService.CrearPersona(datos);
            return Ok(res);
        }
    }
}
