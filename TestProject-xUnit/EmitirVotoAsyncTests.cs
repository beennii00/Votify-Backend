using BusinessLogic.Services;
using Domain.Entitites;
using Microsoft.EntityFrameworkCore;
using Persistance.Data;
using Persistance.Repositories;
using Shared.DTO;
using Shared.Enums;
using Domain.State;

namespace TestProject_xUnit;


public class EmitirVotoAsyncTests
{
    // ── Helpers de infraestructura ────────────────────────────

    /// <summary>
    /// Crea una instancia de AppDbContext in-memory con un nombre
    /// único por test para garantizar aislamiento entre pruebas.
    /// </summary>
    private static AppDbContext CrearContexto(string nombre) =>
        new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(nombre)
                .Options);

    /// <summary>Crea un evento mínimo para poder instanciar Votacion.</summary>
    private static EstandarEvento CrearEvento() =>
        new EstandarEvento("Evento Test", "Desc", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));

    /// <summary>Crea una votación activa y válida por defecto.</summary>
    private static Votacion CrearVotacionValida(bool esPopular = true)
    {
        var vot = new Votacion
        {
            Nombre = "Votación de prueba",
            FechaInicio = DateTime.UtcNow.AddHours(-1),
            FechaFin = DateTime.UtcNow.AddHours(2),
            MaxVotesPerVoter = 3,
            EsVotacionPopular = esPopular,
            TipoVotacion = TipoVotacion.Numerica,
            ValorMaximoNumerico = 10,
            EstadoComentarios = EstadoComentarios.Opcionales,
            evento = CrearEvento()
        };
        vot.SetEstado(new EstadoActiva());
        return vot;
    }

    private static Jurado CrearJurado() => new Jurado("TestJurado", "12345678A", "pass");

    private static EmitirVotoDto CrearDto(int votacionId = 1, int usuarioId = 0) => new EmitirVotoDto
    {
        VotacionId = votacionId,
        UsuarioId = usuarioId,
        VotosProyectos = new List<VotoProyectoDto>()
    };

    // ── R1: Validar Estado de Votación ────────────────────────

    [Fact]
    public async Task EmitirVoto_VotacionPausada_LanzaArgumentException()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(EmitirVoto_VotacionPausada_LanzaArgumentException));
        var votacion = CrearVotacionValida();
        votacion.SetEstado(new EstadoPausada());
        ctx.Votaciones.Add(votacion);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = CrearDto(votacionId: votacion.Id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.EmitirVotoAsync(dto));

        Assert.Contains("pausada", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EmitirVoto_VotacionCerrada_LanzaArgumentException()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(EmitirVoto_VotacionCerrada_LanzaArgumentException));
        var votacion = CrearVotacionValida();
        votacion.SetEstado(new EstadoCerrada());
        ctx.Votaciones.Add(votacion);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = CrearDto(votacionId: votacion.Id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.EmitirVotoAsync(dto));

        Assert.Contains("finalizado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EmitirVoto_FechaFinExpirada_LanzaArgumentException()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(EmitirVoto_FechaFinExpirada_LanzaArgumentException));
        var votacion = CrearVotacionValida();
        votacion.FechaFin = DateTime.UtcNow.AddHours(-1); // Ya expiró
        ctx.Votaciones.Add(votacion);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = CrearDto(votacionId: votacion.Id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.EmitirVotoAsync(dto));

        Assert.Contains("finalizado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── R2: Validar y Obtener Usuario ─────────────────────────

    [Fact]
    public async Task EmitirVoto_UsuarioIdPositivoPeroNoExiste_LanzaArgumentException()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(EmitirVoto_UsuarioIdPositivoPeroNoExiste_LanzaArgumentException));
        var votacion = CrearVotacionValida();
        ctx.Votaciones.Add(votacion);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = CrearDto(votacionId: votacion.Id, usuarioId: 999); // userId > 0 pero no existe

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.EmitirVotoAsync(dto));

        Assert.Contains("Usuario no encontrado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EmitirVoto_VotacionNoPublicaSinUsuario_LanzaArgumentException()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(EmitirVoto_VotacionNoPublicaSinUsuario_LanzaArgumentException));
        var votacion = CrearVotacionValida(esPopular: false); // No popular → requiere usuario
        ctx.Votaciones.Add(votacion);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = CrearDto(votacionId: votacion.Id, usuarioId: 0); // Anónimo

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.EmitirVotoAsync(dto));

        Assert.Contains("pública", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── R3: Validar Autorización de Jurado ────────────────────

    [Fact]
    public async Task EmitirVoto_UsuarioRegistradoNoEsJurado_LanzaArgumentException()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(EmitirVoto_UsuarioRegistradoNoEsJurado_LanzaArgumentException));
        var jurado = CrearJurado();
        var votacion = CrearVotacionValida(esPopular: false);
        // JuradosAsignados vacío → el usuario no está en la lista

        ctx.Users.Add(jurado);
        ctx.Votaciones.Add(votacion);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = CrearDto(votacionId: votacion.Id, usuarioId: jurado.Id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.EmitirVotoAsync(dto));

        Assert.Contains("autorizado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── R4: Validar Duplicidad de Voto ────────────────────────

    [Fact]
    public async Task EmitirVoto_UsuarioYaVoto_LanzaArgumentException()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(EmitirVoto_UsuarioYaVoto_LanzaArgumentException));
        var jurado = CrearJurado();
        var votacion = CrearVotacionValida(esPopular: false);
        votacion.JuradosAsignados = new List<Jurado> { jurado };

        ctx.Users.Add(jurado);
        ctx.Votaciones.Add(votacion);
        await ctx.SaveChangesAsync();

        // Creamos un voto previo para simular que ya votó
        var proyecto = new ProyectoEstandar("P1", "D1", votacion.evento);
        ctx.Projects.Add(proyecto);
        await ctx.SaveChangesAsync();

        var votoExistente = new Voto(DateTime.UtcNow.AddMinutes(-10), jurado, votacion, proyecto, 5);
        ctx.Votes.Add(votoExistente);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = CrearDto(votacionId: votacion.Id, usuarioId: jurado.Id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.EmitirVotoAsync(dto));

        Assert.Contains("emitido", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── R5: Validar Límite de Votos ───────────────────────────

    [Fact]
    public async Task EmitirVoto_SuperaLimiteVotosPermitidos_LanzaArgumentException()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(EmitirVoto_SuperaLimiteVotosPermitidos_LanzaArgumentException));
        var votacion = CrearVotacionValida();
        votacion.MaxVotesPerVoter = 2; // Máximo 2 proyectos
        ctx.Votaciones.Add(votacion);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = CrearDto(votacionId: votacion.Id);
        // 3 votos > MaxVotesPerVoter (2)
        dto.VotosProyectos = new List<VotoProyectoDto>
        {
            new() { ProyectoId = 1, Valoracion = 5 },
            new() { ProyectoId = 2, Valoracion = 7 },
            new() { ProyectoId = 3, Valoracion = 8 },
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.EmitirVotoAsync(dto));

        Assert.Contains("Máximo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── Caso feliz ────────────────────────────────────────────

    [Fact]
    public async Task EmitirVoto_VotacionPopularAnonimaListaVacía_RetornaTrue()
    {
        // Arrange — votación popular activa, lista de proyectos vacía → pasa validaciones y retorna true
        using var ctx = CrearContexto(nameof(EmitirVoto_VotacionPopularAnonimaListaVacía_RetornaTrue));
        var votacion = CrearVotacionValida(esPopular: true);
        ctx.Votaciones.Add(votacion);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = CrearDto(votacionId: votacion.Id, usuarioId: 0);
        dto.VotosProyectos = new List<VotoProyectoDto>(); // Sin proyectos → flujo completo sin fallar

        // Act
        var resultado = await service.EmitirVotoAsync(dto);

        // Assert
        Assert.True(resultado);
    }
}
