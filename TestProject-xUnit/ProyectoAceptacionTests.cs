/*using API.Controllers;
using Shared.DTO;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;

namespace TestProject_xUnit;

public class ProyectoAceptacionTests
{
    [Fact]
    public async Task CrearProyecto_ConDatosValidos_DebeResponderCreatedYRetornarProyectoCreado()
    {
        // Arrange
        var service = new FakeProyectoService();
        var controller = new ProyectoController(service);

        var dto = new EstandarProyectoDTO
        {
            Nombre = "Proyecto Atlas",
            Descripcion = "Plataforma para desafios",
            EventoId = 10
        };

        // Act
        var actionResult = await controller.CrearProyecto(dto);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var body = Assert.IsType<EstandarProyectoDTO>(created.Value);
        Assert.True(body.Id > 0);
        Assert.Equal("Proyecto Atlas", body.Nombre);
        Assert.Equal("Plataforma para desafios", body.Descripcion);
        Assert.Equal(10, body.EventoId);
    }

    [Fact]
    public async Task CrearProyecto_CuandoModelStateEsInvalido_DebeResponderBadRequest()
    {
        // Arrange
        var service = new FakeProyectoService();
        var controller = new ProyectoController(service);
        controller.ModelState.AddModelError("Nombre", "El nombre es obligatorio");

        var dto = new EstandarProyectoDTO
        {
            Nombre = "",
            Descripcion = "Desc",
            EventoId = 1
        };

        // Act
        var actionResult = await controller.CrearProyecto(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task EditarProyecto_ConIdValido_DebeReflejarCambiosEnElListado()
    {
        // Arrange
        var service = new FakeProyectoService();
        var controller = new ProyectoController(service);

        var creado = await controller.CrearProyecto(new EstandarProyectoDTO
        {
            Nombre = "Proyecto Inicial",
            Descripcion = "Desc inicial",
            EventoId = 20
        });

        var createdResult = Assert.IsType<CreatedAtActionResult>(creado.Result);
        var dtoCreado = Assert.IsType<EstandarProyectoDTO>(createdResult.Value);

        var dtoEditado = new EstandarProyectoDTO
        {
            Id = dtoCreado.Id,
            Nombre = "Proyecto Editado",
            Descripcion = "Desc editada",
            EventoId = dtoCreado.EventoId
        };

        // Act
        var updateResult = await controller.ActualizarProyecto(dtoCreado.Id, dtoEditado);
        var listadoResult = await controller.GetProyectosPorEvento(dtoCreado.EventoId);

        // Assert
        var okUpdate = Assert.IsType<OkObjectResult>(updateResult.Result);
        var bodyUpdate = Assert.IsType<EstandarProyectoDTO>(okUpdate.Value);
        Assert.Equal("Proyecto Editado", bodyUpdate.Nombre);

        var okListado = Assert.IsType<OkObjectResult>(listadoResult.Result);
        var proyectos = Assert.IsType<List<ProyectoDTO>>(okListado.Value);
        Assert.Contains(proyectos, p => p.Id == dtoCreado.Id && p.Nombre == "Proyecto Editado");
    }

    [Fact]
    public async Task EditarProyecto_DebeRetornarDatosPreRellenados_AlConsultarPorId()
    {
        // Arrange
        var service = new FakeProyectoService();
        var controller = new ProyectoController(service);

        var creado = await controller.CrearProyecto(new EstandarProyectoDTO
        {
            Nombre = "Proyecto Formulario",
            Descripcion = "Datos para editar",
            EventoId = 40
        });

        var createdResult = Assert.IsType<CreatedAtActionResult>(creado.Result);
        var dtoCreado = Assert.IsType<EstandarProyectoDTO>(createdResult.Value);

        // Act
        var result = await controller.GetProyecto(dtoCreado.Id);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<EstandarProyectoDTO>(ok.Value);
        Assert.Equal(dtoCreado.Id, body.Id);
        Assert.Equal("Proyecto Formulario", body.Nombre);
        Assert.Equal("Datos para editar", body.Descripcion);
        Assert.Equal(40, body.EventoId);
    }

    [Fact]
    public async Task EditarProyecto_CuandoIdUrlNoCoincideConBody_DebeResponderBadRequest()
    {
        // Arrange
        var service = new FakeProyectoService();
        var controller = new ProyectoController(service);

        var dto = new EstandarProyectoDTO
        {
            Id = 9,
            Nombre = "Proyecto",
            Descripcion = "Desc",
            EventoId = 1
        };

        // Act
        var result = await controller.ActualizarProyecto(99, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task EliminarProyecto_Existente_DebeResponderNoContentYNoAparecerEnListado()
    {
        // Arrange
        var service = new FakeProyectoService();
        var controller = new ProyectoController(service);

        var creado = await controller.CrearProyecto(new EstandarProyectoDTO
        {
            Nombre = "Proyecto Temporal",
            Descripcion = "A eliminar",
            EventoId = 30
        });

        var createdResult = Assert.IsType<CreatedAtActionResult>(creado.Result);
        var dtoCreado = Assert.IsType<EstandarProyectoDTO>(createdResult.Value);

        // Act
        var deleteResult = await controller.EliminarProyecto(dtoCreado.Id);
        var listado = await controller.GetProyectosPorEvento(dtoCreado.EventoId);

        // Assert
        Assert.IsType<NoContentResult>(deleteResult);
        var notFound = Assert.IsType<NotFoundObjectResult>(listado.Result);
        Assert.Equal("No se encontraron proyectos para el evento especificado.", notFound.Value);
    }

    [Fact]
    public async Task EliminarProyecto_Inexistente_DebeResponderNotFound()
    {
        // Arrange
        var service = new FakeProyectoService();
        var controller = new ProyectoController(service);

        // Act
        var result = await controller.EliminarProyecto(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CrearProyecto_CuandoServicioNoCrea_DebeResponderBadRequest()
    {
        // Arrange
        var service = new FakeProyectoService();
        var controller = new ProyectoController(service);

        var dto = new EstandarProyectoDTO
        {
            Nombre = "Proyecto sin evento valido",
            Descripcion = "Desc",
            EventoId = 0
        };

        // Act
        var actionResult = await controller.CrearProyecto(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact(Skip = "Pendiente: el criterio de aceptacion pide descripcion opcional, pero ProyectoDTO la marca como obligatoria.")]
    public void CrearProyecto_SinDescripcion_DebePermitirAlta() { }

    [Fact(Skip = "Pendiente: el criterio de aceptacion pide categoria y numero de concursantes en listado, pero el DTO actual no los expone.")]
    public void ListadoProyecto_DebeMostrarCategoriaYNumeroConcursantes() { }

    [Fact(Skip = "Pendiente: la restriccion de solo administrador no esta implementada en ProyectoController.")]
    public void SoloAdmin_PuedeCrearEditarEliminarProyectos() { }

    private sealed class FakeProyectoService : IProyectoService
    {
        private readonly List<ProyectoDTO> _data = new();
        private int _nextId = 1;

        public Task<List<ProyectoDTO>> ListarProyectosPorEventoAsync(int eventoId)
        {
            var result = _data
                .Where(p => p.EventoId == eventoId)
                .Select(Clone)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<ProyectoDTO> ObtenerProyectoPorIdAsync(int id)
        {
            var encontrado = _data.FirstOrDefault(p => p.Id == id);
            if (encontrado is null)
            {
                return Task.FromResult<ProyectoDTO>(null!);
            }

            return Task.FromResult<ProyectoDTO>(Clone(encontrado));
        }

        public Task<ProyectoDTO> CrearProyectoAsync(ProyectoDTO proyectoDto)
        {
            if (proyectoDto.EventoId <= 0)
            {
                return Task.FromResult<ProyectoDTO>(null!);
            }

            var nuevo = Clone(proyectoDto);
            nuevo.Id = _nextId++;
            _data.Add(nuevo);
            return Task.FromResult(Clone(nuevo));
        }

        public Task<ProyectoDTO> ActualizarProyectoAsync(ProyectoDTO proyectoDto)
        {
            var existente = _data.FirstOrDefault(p => p.Id == proyectoDto.Id);
            if (existente is null)
            {
                return Task.FromResult<ProyectoDTO>(null!);
            }

            existente.Nombre = proyectoDto.Nombre;
            existente.Descripcion = proyectoDto.Descripcion;
            existente.EventoId = proyectoDto.EventoId;

            return Task.FromResult(Clone(existente));
        }

        public Task<bool> EliminarProyectoAsync(int id)
        {
            var existente = _data.FirstOrDefault(p => p.Id == id);
            if (existente is null)
            {
                return Task.FromResult(false);
            }

            _data.Remove(existente);
            return Task.FromResult(true);
        }

        private static ProyectoDTO Clone(ProyectoDTO dto)
        {
            return new EstandarProyectoDTO
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                EventoId = dto.EventoId
            };
        }
    }
}
*/
