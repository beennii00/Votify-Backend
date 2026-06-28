using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistance.Data;

namespace TestProject_xUnit;

public class EventoApiHttpAceptacionTests : IClassFixture<EventoApiFactory>
{
    private readonly HttpClient _client;

    public EventoApiHttpAceptacionTests(EventoApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostEvento_ConPayloadValido_DebeRetornar201Created()
    {
        // Arrange
        var payload = new
        {
            tipoEvento = "Estandar",
            nombre = "Hackathon HTTP",
            descripcion = "Evento creado por prueba de aceptacion",
            fechaInicio = DateTime.UtcNow.AddDays(1),
            fechaFin = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/evento", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<EventoResponseDto>();
        Assert.NotNull(body);
        Assert.True(body!.Id > 0);
        Assert.Equal("Hackathon HTTP", body.Nombre);
    }

    [Fact]
    public async Task PostEvento_ConFechaFinMenorIgualAInicio_DebeRetornar400BadRequest()
    {
        // Arrange
        var payload = new
        {
            tipoEvento = "Estandar",
            nombre = "Evento invalido",
            descripcion = "Prueba validacion",
            fechaInicio = DateTime.UtcNow.AddDays(3),
            fechaFin = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/evento", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetEvento_DespuesDeCrear_DebeRetornar200YContenerElEvento()
    {
        // Arrange
        var payload = new
        {
            tipoEvento = "Estandar",
            nombre = "Evento para listado",
            descripcion = "Debe aparecer en GET /evento",
            fechaInicio = DateTime.UtcNow.AddDays(4),
            fechaFin = DateTime.UtcNow.AddDays(6)
        };

        var createResponse = await _client.PostAsJsonAsync("/evento", payload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Act
        var listResponse = await _client.GetAsync("/evento");

        // Assert
        Assert.Equal(HttpStatusCode.Created, listResponse.StatusCode);

        var eventos = await listResponse.Content.ReadFromJsonAsync<List<EventoResponseDto>>();
        Assert.NotNull(eventos);
        Assert.Contains(eventos!, e => e.Nombre == "Evento para listado");
    }

    private sealed class EventoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}

public sealed class EventoApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"VotifyTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }
}
