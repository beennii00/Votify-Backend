using Shared.DTO;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VotacionController : ControllerBase
    {
        private readonly IVotacionService _votacionService;

        public VotacionController(IVotacionService votacionService)
        {
            _votacionService = votacionService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JsonElement body)
        {
            VotacionFormDto dto;
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                opts.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                dto = JsonSerializer.Deserialize<VotacionFormDto>(body.GetRawText(), opts)!;
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Invalid JSON", details = ex.Message });
            }

            if (!dto.AgregarPremio)
            {
                dto.Premio = null;
            }

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(dto);
            var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);
            if (!isValid)
            {
                ModelState.Clear();
                foreach (var vr in validationResults)
                {
                    var members = vr.MemberNames != null && vr.MemberNames.Any() ? vr.MemberNames : new[] { string.Empty };
                    foreach (var m in members)
                        ModelState.AddModelError(m, vr.ErrorMessage ?? string.Empty);
                }
                return BadRequest(ModelState);
            }

            try
            {
                var votacion = await _votacionService.CreateAsync(dto);
                return Ok(votacion);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocurrió un error al crear la votación.", details = ex.Message });
            }
        }

        [HttpPut("{eventoId}/{votacionId}")]
        public async Task<IActionResult> Update(int eventoId, int votacionId, [FromBody] JsonElement body)
        {
            VotacionFormDto dto;
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                opts.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                dto = JsonSerializer.Deserialize<VotacionFormDto>(body.GetRawText(), opts)!;
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Invalid JSON", details = ex.Message });
            }

            if (!dto.AgregarPremio)
            {
                dto.Premio = null;
            }

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(dto);
            var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);
            if (!isValid)
            {
                ModelState.Clear();
                foreach (var vr in validationResults)
                {
                    var members = vr.MemberNames != null && vr.MemberNames.Any() ? vr.MemberNames : new[] { string.Empty };
                    foreach (var m in members)
                        ModelState.AddModelError(m, vr.ErrorMessage ?? string.Empty);
                }
                return BadRequest(ModelState);
            }

            try
            {
                var votacion = await _votacionService.UpdateAsync(eventoId, votacionId, dto);
                return Ok(votacion);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocurrió un error al actualizar la votación.", details = ex.Message });
            }
        }

        [HttpDelete("{eventoId}/{votacionId}")]
        public async Task<IActionResult> Delete(int eventoId, int votacionId)
        {
            try
            {
                await _votacionService.DeleteAsync(eventoId, votacionId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocurrió un error al eliminar la votación.", details = ex.Message });
            }
        }
        [HttpGet("acceso/{codigo}")]
        public async Task<IActionResult> ValidarCodigo(string codigo)
        {
            try
            {
                var result = await _votacionService.ValidarCodigoAccesoAsync(codigo);
                return Ok(new { eventoId = result.EventoId, votacionId = result.VotacionId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al validar el código.", details = ex.Message });
            }
        }
        [HttpGet("{eventoId}/{votacionId}/detalle")]
        public async Task<IActionResult> GetDetalle(int eventoId, int votacionId)
        {
            try
            {
                var response = await _votacionService.GetDetalleVotacionAsync(eventoId, votacionId);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocurrió un error al obtener el detalle.", details = ex.Message });
            }
        }

        [HttpGet("{eventoId}/{votacionId}/proyecto/{proyectoId}/resumen")]
        public async Task<IActionResult> GetResumenIA(int eventoId, int votacionId, int proyectoId)
        {
            try
            {
                var response = await _votacionService.GetResumenIAAsync(eventoId, votacionId, proyectoId);
                return Ok(new { Resumen = response });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocurrio un error al obtener el resumen.", details = ex.Message });
            }
        }

        [HttpPost("{eventoId}/{votacionId}/accion")]
        public async Task<IActionResult> AccionVotacion(int eventoId, int votacionId, [FromQuery] string accion)
        {
            try
            {
                var response = await _votacionService.AccionVotacionAsync(eventoId, votacionId, accion);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocurrió un error al procesar la acción.", details = ex.Message });
            }
        }


        [HttpPost("{votacionId}/asignar-jurados")]
        public async Task<IActionResult> AsignarJurados(int votacionId, [FromBody] AsignarJuradosDto dto)
        {
            if (votacionId != dto.VotacionId)
            {
                return BadRequest(new { error = "El ID de la votación en la ruta no coincide con el del cuerpo." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _votacionService.AsignarJuradosAsync(dto);
                return Ok(new { success });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocurrió un error al asignar jurados.", details = ex.Message });
            }
        }

        [HttpGet("{eventoId}/{votacionId}/ya-vote")]
        public async Task<IActionResult> YaVoto(int eventoId, int votacionId)
        {
            var userIdStr = Request.Headers["X-User-Id"].FirstOrDefault()
                            ?? Request.Query["userId"].FirstOrDefault();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "User ID requerido." });

            var yaVoto = await _votacionService.YaVotoAsync(votacionId, userId);
            return Ok(new { yaVoto });
        }

        [HttpGet("mis-votaciones")]
        public async Task<IActionResult> GetMisVotaciones()
        {
            // Fix: We can't use [Authorize] or User.Claims natively right now in this codebase
            // because AddAuthentication/AddJwtBearer isn't configured.
            // Instead of using real Claims from headers (requiring JwtBearer integration),
            // We expect the frontend to either pass UserId as parameter or from a header explicitly
            // until full Auth is configured in API Program.cs
            
            // For now, let's extract user ID from query or custom header.
            var userIdStr = Request.Headers["X-User-Id"].FirstOrDefault() 
                            ?? Request.Query["userId"].FirstOrDefault();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "User ID is missing. Add 'userId' query parameter or 'X-User-Id' header." });

            var votaciones = await _votacionService.GetVotacionesResumenByJuradoAsync(userId);
            return Ok(votaciones);
        }
    }
}