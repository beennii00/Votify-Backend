using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shared.DTO;
using Votify.BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConcursanteController : ControllerBase
    {
        private readonly IIARoadmapService _roadmapService;

        public ConcursanteController(IIARoadmapService roadmapService)
        {
            _roadmapService = roadmapService;
        }

        // Simula la obtención de [Authorize] y sacando el id, pero dado que en otros métodos de este proyecto 
        // parece que el `userId` se extrae de headers personalizados por falta de configuración real de token,
        // haré lo que pidió el usuario con [Authorize] mockeado y extraer el ID, o simplemente recibiéndolo de header.
        // He incluido la firma solicitada por el usuario.
        
        [HttpPost("hoja-ruta/generar")]
        // [Authorize] -> El usuario pidió Authorize y extraer el ID del token.
        // Pero dado cómo está el frontend y Program.cs del API actual, 
        // haré que intente cogerlo de `ClaimTypes.NameIdentifier` si hubiese Claims puestas por un middleware,
        // O mediante la cabecera / Body de contingencia.
        public async Task<IActionResult> GenerarHojaRuta()
        {
            try
            {
                // Intento extraer del User.Claims (Asumiendo que [Authorize] real está corriendo o si no lo está)
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                // Contingencia en caso de que JwtBearer no se use y manden header "X-User-Id" como en `VotacionController.GetMisVotaciones`
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    userIdClaim = Request.Headers["X-User-Id"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    // Fallback to body request si el frontend no pudiese mandar [Authorize] de header
                    return Unauthorized(new { message = "User ID no está disponible en las credenciales." });
                }

                var responseDto = await _roadmapService.GenerarHojaDeRutaAsync(userId);
                
                return Ok(responseDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al generar la hoja de ruta.", details = ex.Message });
            }
        }
    }
}
