using Shared.DTO;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventoController : ControllerBase
    {
        private readonly IEventoService _eventoService;

        public EventoController(IEventoService eventoService)
        {
            _eventoService = eventoService;
        }

        // GET /evento — listado de eventos 
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var eventos = await _eventoService.GetAllAsync();
            return Ok(eventos);
        }

        // POST /evento — crear nuevo evento (solo admin)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventoDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var evento = await _eventoService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = evento.Id }, evento);
            }
            catch (ArgumentException ex)
            {
                // Error de validación: fecha fin anterior a fecha inicio
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET /evento/{id} — Traer un evento para pre-rellenar el formulario
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                EventoDTO evento = await _eventoService.GetByIdAsync(id);

                // comportamiento un poco traicionero de ASP.NET Core, lo serializo manualmente pq sino no se introduce el atributo tipo
                // en el json. 
                var json = System.Text.Json.JsonSerializer.Serialize<EventoDTO>(evento);
                return Content(json, "application/json");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // PUT /evento/{id} — Editar un evento existente (UT 3324)
        [HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] EventoDTO dto)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			try
			{
				var eventoActualizado = await _eventoService.UpdateAsync(id, dto);
				return Ok(eventoActualizado);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { error = ex.Message });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		// DELETE /evento/{id} — Eliminar evento (UT 3325)
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var borrado = await _eventoService.DeleteAsync(id);
				if (!borrado) return NotFound(new { error = "Evento no encontrado." });

				return Ok(new { mensaje = "Evento eliminado y desvinculado correctamente, c'est carré." });
			}
			catch (InvalidOperationException ex)
			{
				// Aquí salta la restricción de las votaciones activas
				return BadRequest(new { error = ex.Message });
			}
		}
	}
}