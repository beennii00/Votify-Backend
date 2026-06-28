using Shared.DTO;
using BusinessLogic.Services;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using API.Hubs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoteController : ControllerBase
    {
        private readonly IVoteService _voteService;
        private readonly IProyectoService _proyectoService;
        private readonly ISujetoVotacion _sujetoVotacion;

        public VoteController(IVoteService voteService, IProyectoService proyectoService, ISujetoVotacion sujetoVotacion)
        {
            _voteService = voteService;
            _proyectoService = proyectoService;
            _sujetoVotacion = sujetoVotacion;
        }

        [HttpPost("configurar-fechas")]
        public async Task<IActionResult> ConfigurarFechas([FromBody] ConfigurarFechasVotacionDto dto)
        {
            try
            {
                var resultado = await _voteService.ConfigurarFechasVotacionAsync(dto);

                if (!resultado)
                {
                    return NotFound(new { mensaje = "Votación no encontrada." });
                }

                return Ok(new { mensaje = "Fechas configuradas exitosamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Hubo un fallo en el servidor: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerVotacion(int id)
        {
            var votacion = await _voteService.ObtenerVotacionAsync(id);

            if (votacion == null)
            {
                return NotFound(new { mensaje = "Votación no encontrada." });
            }

            return Ok(votacion);
        }

        [HttpGet("proyectos/{eventoId}")]
        public async Task<IActionResult> ObtenerProyectosAEvaluar(int eventoId)
        {
            var proyectos = await _proyectoService.ListarProyectosPorEventoAsync(eventoId);
            return Ok(proyectos.Select(p => new {
                p.Id,
                p.Nombre,
                p.Descripcion
            }));
        }

        [HttpPost("emitir")]
        public async Task<IActionResult> EmitirVoto([FromBody] EmitirVotoDto dto)
        {
            if (dto == null || dto.VotosProyectos == null || !dto.VotosProyectos.Any())
            {
                return BadRequest("Datos de voto inválidos.");
            }

            try
            {
                var result = await _voteService.EmitirVotoAsync(dto);
                if (result) 
                {
                    // Patrón Observador: El objeto que cambia de estado invoca al Sujeto para que Notifique
                    await _sujetoVotacion.NotificarCambioVotos(dto.VotacionId);

                    return Ok(new { message = "Votos registrados correctamente." });
                }
                return BadRequest("No se pudo registrar el voto. Verifique el usuario/votación.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
