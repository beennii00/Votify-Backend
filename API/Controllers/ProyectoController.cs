using Shared.DTO;
using BusinessLogic.Services;
using Domain.Entitites;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ProyectoController : ControllerBase
    {
        //Atributos que necesito 

        private readonly IProyectoService _proyectoService;
        public ProyectoController(IProyectoService proyectoService)
        {
            _proyectoService = proyectoService;
        }


        //GET : Obtengo la lista de proyecto 
        [HttpGet("/api/eventos/{eventoId}/proyectos")]
        public async Task<ActionResult<List<ProyectoDTO>>> GetProyectosPorEvento(int eventoID)
        {
            var proyectos = await _proyectoService.ListarProyectosPorEventoAsync(eventoID);

            if (proyectos == null || proyectos.Count == 0)
            {
                return NotFound("No se encontraron proyectos para el evento especificado.");
            }
            //OJO: NO USAR  Ok(proyectos) porque sino el serializador no introduce el atributo disciminante de tipo. 
            return proyectos;


        }

        //GET Obtengo detalles del proyecto por su id

        [HttpGet("{idP}")]
        public async Task<ActionResult<ProyectoDTO>> GetProyecto(int idP)
        {
            var proyecto = await _proyectoService.ObtenerProyectoPorIdAsync(idP);
            if (proyecto == null)
            {
                return NotFound("No se encontr� el proyecto con el ID especificado.");
            }

            //OJO: NO USAR  Ok(proyectos) 
            return proyecto;
        }

        //Post : api/proyecto - Creo un nuevo proyecto
        [HttpPost]
        public async Task<ActionResult<ProyectoDTO>> CrearProyecto([FromBody] ProyectoDTO miDto)
        {
            //si el modelo no es valido 
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var proyNew = await _proyectoService.CrearProyectoAsync(miDto);

            if (proyNew is null)
            {
                return BadRequest("No se pudo crear el proyecto. Verifique los datos proporcionados.");
            }

            //lo creo y lo retorno con un CreatedAtAction para que el cliente pueda obtener la ubicaci�n del nuevo recurso
            return CreatedAtAction(nameof(GetProyecto), new { idP = proyNew.Id }, proyNew);

        }

        //PUT:  api/proyecto/5 - Para actualizar un proyecto existente
        [HttpPut("{idP}")]
        public async Task<ActionResult<ProyectoDTO>> ActualizarProyecto(int idP, [FromBody] ProyectoDTO miNuevoDto)
        {

            if (idP != miNuevoDto.Id)
            {
                return BadRequest("El ID del proyecto en la URL no coincide con el ID del proyecto a modificar.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var proyectoActualizado = await _proyectoService.ActualizarProyectoAsync(miNuevoDto);

            if (proyectoActualizado == null)
            {
                return NotFound($"No se encontr� ning�n proyecto con el ID {idP} para actualizar.");
            }
            //OJO: NO USAR  Ok(proyectos) 
            return (proyectoActualizado);
        }


        //DELETE : api/proyecto/5 - Para eliminar un proyecto por su ID
        [HttpDelete("{idP}")]
        public async Task<IActionResult> EliminarProyecto(int idP)
        {


            var res = await _proyectoService.EliminarProyectoAsync(idP);

            if (res)
            {
                return NoContent();
            }
            else
            {
                return NotFound($"No se encontr� ning�n proyecto con el ID {idP} para eliminar.");
            }

        }

		[HttpPost("equipo/concursantes")]
		public async Task<IActionResult> AsignarConcursantesEquipo([FromBody] AsignarConcursanteDto dto)
		{
			try
			{
				if (dto == null || dto.ProyectoId <= 0)
				{
					return BadRequest(new { message = "Datos inv�lidos. Verifique la informaci�n enviada." });
				}

				await _proyectoService.AsignarEquipoAsync(dto);
				return Ok(new { message = "Equipo actualizado correctamente." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = "Se ha producido un error interno al asignar el equipo.", details = ex.Message });
			}
		}

		[HttpGet("equipo/{proyectoId}")]
		public async Task<IActionResult> ObtenerEquipoProyecto(int proyectoId)
		{
			try
			{
				var equipo = await _proyectoService.ObtenerEquipoProyectoAsync(proyectoId);
				return Ok(equipo);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = "Se ha producido un error interno al obtener el equipo del proyecto.", details = ex.Message });
			}
		}

        [HttpGet("test-prueba")]
        public ActionResult<ProyectoDTO> TestPrueba()
        {
            ProyectoDTO prueba = new EstandarProyectoDTO
            {
                Id = 999,
                Nombre = "Prueba Pura",
                Descripcion = "Probando el serializador",
                EventoId = 1
            };

            return prueba;
        }




    }
}
