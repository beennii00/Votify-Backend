using System.Net;
using System.Net.Http.Json;
using Shared.DTO;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TestProject_xUnit;

public class ProyectoApiHttpAceptacionTests : IClassFixture<EventoApiFactory>
{
    private readonly HttpClient _client;

    public ProyectoApiHttpAceptacionTests(EventoApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostProyecto_ConPayloadValido_DebeRetornar201Created()
    {
        // Arrange
        var eventoId = await CrearEventoMarcoAsync();

        var payload = new
        {
            tipoProyecto = "estandar",
            nombre = $"Proyecto HTTP {Guid.NewGuid():N}",
            descripcion = "Proyecto de prueba de aceptacion",
            eventoId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Proyecto", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<EstandarProyectoDTO>();
        Assert.NotNull(body);
        Assert.True(body!.Id > 0);
        Assert.Equal(payload.nombre, body.Nombre);
        Assert.Equal(payload.descripcion, body.Descripcion);
        Assert.Equal(eventoId, body.EventoId);
    }

    [Fact]
    public async Task GetProyectoPorId_DespuesDeCrear_DebeRetornar200Ok()
    {
        // Arrange
        var proyectoId = await CrearProyectoAsync();

        // Act
        var response = await _client.GetAsync($"/api/Proyecto/{proyectoId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<EstandarProyectoDTO>();
        Assert.NotNull(body);
        Assert.Equal(proyectoId, body!.Id);
    }

    [Fact]
    public async Task GetProyectosPorEvento_DebeRetornar200YContenerProyectoCreado()
    {
        // Arrange
        var eventoId = await CrearEventoMarcoAsync();
        var nombre = $"Proyecto listado {Guid.NewGuid():N}";

        var payload = new
        {
            tipoProyecto = "estandar",
            nombre,
            descripcion = "Debe aparecer en listado por evento",
            eventoId
        };

        var postResponse = await _client.PostAsJsonAsync("/api/Proyecto", payload);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        // Act
        var response = await _client.GetAsync($"/api/eventos/{eventoId}/proyectos");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<EstandarProyectoDTO>>();
        Assert.NotNull(body);
        Assert.Contains(body!, p => p.Nombre == nombre && p.EventoId == eventoId);
    }

    [Fact]
    public async Task PutProyecto_ConDatosValidos_DebeRetornar200YAplicarCambios()
    {
        // Arrange
        var eventoId = await CrearEventoMarcoAsync();

        var createPayload = new
        {
            tipoProyecto = "estandar",
            nombre = $"Proyecto editado {Guid.NewGuid():N}",
            descripcion = "Descripcion inicial",
            eventoId
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Proyecto", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var creado = await createResponse.Content.ReadFromJsonAsync<EstandarProyectoDTO>();
        Assert.NotNull(creado);
        var proyectoId = creado!.Id;

        var updatePayload = new
        {
            Id = proyectoId,
            Nombre = "Proyecto Editado HTTP",
            Descripcion = "Descripcion editada",
            EventoId = eventoId
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/Proyecto/{proyectoId}", updatePayload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<EstandarProyectoDTO>();
        Assert.NotNull(updated);
        Assert.Equal("Proyecto Editado HTTP", updated!.Nombre);
        Assert.Equal("Descripcion editada", updated.Descripcion);
    }

    [Fact]
    public async Task DeleteProyecto_Existente_DebeRetornar204NoContent()
    {
        // Arrange
        var proyectoId = await CrearProyectoAsync();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/Proyecto/{proyectoId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProyecto_DespuesDeEliminar_GetPorIdDebeRetornar404()
    {
        // Arrange
        var proyectoId = await CrearProyectoAsync();

        var deleteResponse = await _client.DeleteAsync($"/api/Proyecto/{proyectoId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Act
        var getResponse = await _client.GetAsync($"/api/Proyecto/{proyectoId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private async Task<int> CrearProyectoAsync()
    {
        var eventoId = await CrearEventoMarcoAsync();

        var payload = new
        {
            tipoProyecto = "estandar",
            nombre = $"Proyecto base {Guid.NewGuid():N}",
            descripcion = "Proyecto para pruebas de endpoint",
            eventoId
        };

        var response = await _client.PostAsJsonAsync("/api/Proyecto", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<EstandarProyectoDTO>();
        Assert.NotNull(body);
        return body!.Id;
    }

    private async Task<int> CrearEventoMarcoAsync()
    {
        var payload = new
        {
            tipoEvento = "Estandar",
            nombre = $"Evento marco {Guid.NewGuid():N}",
            descripcion = "Evento para asociar proyecto",
            fechaInicio = DateTime.UtcNow.AddDays(1),
            fechaFin = DateTime.UtcNow.AddDays(2)
        };

        var response = await _client.PostAsJsonAsync("/evento", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<EventoCreadoResponse>();
        Assert.NotNull(body);
        return body!.Id;
    }

    private sealed class EventoCreadoResponse
    {
        public int Id { get; set; }
    }
}
