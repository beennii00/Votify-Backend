using API.Controllers;
using Shared.DTO;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace TestProject_xUnit;

public class EventoModificacionUnitTests
{
    [Fact]
    public async Task UpdateEvento_ConDatosValidos_DebeRetornarOkYEventoActualizado()
    {
        // Arrange
        var service = new FakeEventoService();
        var controller = new EventoController(service);

        var dto = new EstandarEventoDto
        {
            Nombre = "Evento Editado",
            Descripcion = "Descripcion editada",
            FechaInicio = DateTime.UtcNow.AddDays(1),
            FechaFin = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var result = await controller.Update(1, dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<EstandarEventoDto>(ok.Value);
        Assert.Equal(1, body.Id);
        Assert.Equal("Evento Editado", body.Nombre);
        Assert.Equal("Descripcion editada", body.Descripcion);
    }

    [Fact]
    public async Task UpdateEvento_CuandoModelStateEsInvalido_DebeRetornarBadRequest()
    {
        // Arrange
        var service = new FakeEventoService();
        var controller = new EventoController(service);
        controller.ModelState.AddModelError("Nombre", "El nombre es obligatorio");

        var dto = new EstandarEventoDto
        {
            Nombre = "",
            Descripcion = "Desc",
            FechaInicio = DateTime.UtcNow.AddDays(1),
            FechaFin = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var result = await controller.Update(1, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateEvento_CuandoNoExiste_DebeRetornarNotFound()
    {
        // Arrange
        var service = new FakeEventoService();
        var controller = new EventoController(service);

        var dto = new EstandarEventoDto
        {
            Nombre = "Evento Inexistente",
            Descripcion = "Desc",
            FechaInicio = DateTime.UtcNow.AddDays(1),
            FechaFin = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var result = await controller.Update(999, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateEvento_CuandoFechaFinNoEsPosterior_DebeRetornarBadRequest()
    {
        // Arrange
        var service = new FakeEventoService();
        var controller = new EventoController(service);

        var dto = new EstandarEventoDto
        {
            Nombre = "Evento Fechas Invalidas",
            Descripcion = "Desc",
            FechaInicio = DateTime.UtcNow.AddDays(2),
            FechaFin = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await controller.Update(1, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateEvento_ConEsportsDto_DebeMantenerTipoYCampos()
    {
        // Arrange
        var service = new FakeEventoService();
        var controller = new EventoController(service);

        var dto = new ESportsEventoDto
        {
            Nombre = "Evento Esports",
            Descripcion = "Final regional",
            FechaInicio = DateTime.UtcNow.AddDays(3),
            FechaFin = DateTime.UtcNow.AddDays(4),
            Juego = "Valorant",
            Plataforma = "PC"
        };

        // Act
        var result = await controller.Update(1, dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<ESportsEventoDto>(ok.Value);
        Assert.Equal("Valorant", body.Juego);
        Assert.Equal("PC", body.Plataforma);
    }

    private sealed class FakeEventoService : IEventoService
    {
        private readonly Dictionary<int, EventoDTO> _eventos = new()
        {
            [1] = new EstandarEventoDto
            {
                Id = 1,
                Nombre = "Evento Base",
                Descripcion = "Descripcion base",
                FechaInicio = DateTime.UtcNow.AddDays(1),
                FechaFin = DateTime.UtcNow.AddDays(2)
            }
        };

        public Task<IEnumerable<EventoDTO>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<EventoDTO>>(_eventos.Values.ToList());
        }

        public Task<EventoDTO> CreateAsync(EventoDTO dto)
        {
            var nuevoId = _eventos.Keys.Max() + 1;
            var creado = Clonar(dto);
            creado.Id = nuevoId;
            _eventos[nuevoId] = creado;
            return Task.FromResult(creado);
        }

        public Task<EventoDTO> GetByIdAsync(int id)
        {
            if (!_eventos.TryGetValue(id, out var evento))
            {
                throw new KeyNotFoundException("Evento no encontrado.");
            }

            return Task.FromResult(Clonar(evento));
        }

        public Task<EventoDTO> UpdateAsync(int id, EventoDTO dto)
        {
            if (!_eventos.ContainsKey(id))
            {
                throw new KeyNotFoundException("Evento no encontrado.");
            }

            if (dto.FechaFin <= dto.FechaInicio)
            {
                throw new ArgumentException("La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            var actualizado = Clonar(dto);
            actualizado.Id = id;
            _eventos[id] = actualizado;

            return Task.FromResult(Clonar(actualizado));
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(_eventos.Remove(id));
        }

        private static EventoDTO Clonar(EventoDTO dto)
        {
            if (dto is ESportsEventoDto esports)
            {
                return new ESportsEventoDto
                {
                    Id = dto.Id,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    FechaInicio = dto.FechaInicio,
                    FechaFin = dto.FechaFin,
                    Juego = esports.Juego,
                    Plataforma = esports.Plataforma,
                    Votacion = dto.Votacion
                };
            }

            return new EstandarEventoDto
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Votacion = dto.Votacion
            };
        }
    }
}
