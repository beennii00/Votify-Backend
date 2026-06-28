using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services;
using Shared.DTO;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUsuarioDTO dto)
        {
            try
            {
                var response = await _usuarioService.RegistrarAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUsuarioDTO dto)
        {
            try
            {
                var response = await _usuarioService.IniciarSesionAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPerfil(int id, [FromBody] UpdateUsuarioDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _usuarioService.ActualizarPerfilAsync(id, dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

		[HttpGet("buscar")]
		public async Task<IActionResult> BuscarConcursantes([FromQuery] string query)
		{
			try
			{
				var resultados = await _usuarioService.BuscarConcursantesAsync(query);
				return Ok(resultados);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = "Se ha producido un error interno al intentar buscar los usuarios.", details = ex.Message });
			}
		}

        [HttpGet("{id}/dashboard")]
        public async Task<IActionResult> GetDashboardConcursante(int id)
        {
            try
            {
                var dashboard = await _usuarioService.ObtenerDashboardConcursanteAsync(id);
                return Ok(dashboard);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Concursante no encontrado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

		[HttpGet("jurados")]
		public async Task<IActionResult> ObtenerJurados()
		{
			try
			{
				var resultados = await _usuarioService.ObtenerTodosLosJuradosAsync();
				return Ok(resultados);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = "Error al obtener los jurados.", details = ex.Message });
			}
		}

		[HttpPost("reset-contrasena")]
		public async Task<IActionResult> ResetContrasena([FromBody] CambiarContrasenyaDTO dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				await _usuarioService.CambiarContrasenyaAsync(dto);
				return NoContent();
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Error al cambiar la contraseña.", details = ex.Message });
			}
		}
	}
}
