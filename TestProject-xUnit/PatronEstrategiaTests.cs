using BusinessLogic.Services;
using Domain.Entitites;
using Microsoft.EntityFrameworkCore;
using Persistance.Data;
using Persistance.Repositories;
using Shared.DTO;
using Shared.Enums;
using Domain.State;

namespace TestProject_xUnit;

/// <summary>
/// Tests de regresión para el Patrón Estrategia aplicado a EmitirVotoAsync.
///
/// Objetivo: certificar que el comportamiento observable de EmitirVotoAsync
/// es IDÉNTICO antes y después de la refactorización al Patrón Estrategia.
/// Cada test ejerce uno de los tres algoritmos (Numerica, Binaria, CriteriosPorPeso)
/// a través del Contexto (VoteService), sin acoplarse a las clases de estrategia
/// internas —tal como exige el patrón de Gamma.
/// </summary>
public class PatronEstrategiaTests
{
    // ── Infraestructura in-memory (igual que EmitirVotoAsyncTests) ─────────

    private static AppDbContext CrearContexto(string nombre) =>
        new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(nombre)
                .Options);

    private static EstandarEvento CrearEvento() =>
        new EstandarEvento("Evento PatronEstrategia", "Desc", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));

    /// <summary>Crea una votación popular activa con el tipo indicado.</summary>
    private static Votacion CrearVotacion(TipoVotacion tipo, int? valorMaximo = null)
    {
        var vot = new Votacion
        {
            Nombre = $"Votación {tipo}",
            FechaInicio = DateTime.UtcNow.AddHours(-1),
            FechaFin = DateTime.UtcNow.AddHours(2),
            MaxVotesPerVoter = 5,
            EsVotacionPopular = true,   // Anónima → sin restriccin de jurado
            TipoVotacion = tipo,
            ValorMaximoNumerico = valorMaximo,
            EstadoComentarios = EstadoComentarios.Desactivados,
            evento = CrearEvento()
        };
        vot.SetEstado(new EstadoActiva());
        return vot;
    }

    private static ProyectoEstandar CrearProyecto(EstandarEvento ev) =>
        new ProyectoEstandar("Proyecto Test", "Desc", ev);

    // ── Caminos felices (Happy Path) ───────────────────────────────────────

    /// <summary>
    /// PE-01: La estrategia Numérica crea y persiste un voto cuando la valoración
    /// es válida y no supera el máximo configurado.
    /// </summary>
    [Fact]
    public async Task Estrategia_Numerica_VotoValido_SePersisteEnBD()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(Estrategia_Numerica_VotoValido_SePersisteEnBD));
        var votacion = CrearVotacion(TipoVotacion.Numerica, valorMaximo: 10);
        var proyecto = CrearProyecto((EstandarEvento)votacion.evento);
        ctx.Votaciones.Add(votacion);
        ctx.Projects.Add(proyecto);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = new EmitirVotoDto
        {
            VotacionId = votacion.Id,
            UsuarioId = 0,
            VotosProyectos = new List<VotoProyectoDto>
            {
                new() { ProyectoId = proyecto.Id, Valoracion = 7 }
            }
        };

        // Act
        var resultado = await service.EmitirVotoAsync(dto);

        // Assert
        Assert.True(resultado);
        var votosEnBd = ctx.Votes.Count();
        Assert.Equal(1, votosEnBd);
    }

    /// <summary>
    /// PE-02: La estrategia Binaria crea y persiste un voto cuando el valor es 1.
    /// </summary>
    [Fact]
    public async Task Estrategia_Binaria_VotoValido_Valor1_SePersisteEnBD()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(Estrategia_Binaria_VotoValido_Valor1_SePersisteEnBD));
        var votacion = CrearVotacion(TipoVotacion.Binaria);
        var proyecto = CrearProyecto((EstandarEvento)votacion.evento);
        ctx.Votaciones.Add(votacion);
        ctx.Projects.Add(proyecto);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = new EmitirVotoDto
        {
            VotacionId = votacion.Id,
            UsuarioId = 0,
            VotosProyectos = new List<VotoProyectoDto>
            {
                new() { ProyectoId = proyecto.Id, Valoracion = 1 }
            }
        };

        // Act
        var resultado = await service.EmitirVotoAsync(dto);

        // Assert
        Assert.True(resultado);
        Assert.Equal(1, ctx.Votes.Count());
    }

    /// <summary>
    /// PE-03: La estrategia CriteriosPorPeso pondera correctamente los criterios
    /// y persiste un único voto con el valor ponderado calculado.
    /// </summary>
    [Fact]
    public async Task Estrategia_CriteriosPorPeso_VotoValido_SePersisteConPonderacion()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(Estrategia_CriteriosPorPeso_VotoValido_SePersisteConPonderacion));
        var votacion = CrearVotacion(TipoVotacion.CriteriosPorPeso);

        // Criterio 1: peso 0.6, valoracion 8 → contribución 4.8
        // Criterio 2: peso 0.4, valoracion 5 → contribución 2.0
        // Total ponderado: 6.8 → redondeado a 7
        var criterio1 = new CriterioVotacion { Nombre = "Innovación", Peso = 0.6m, VotacionId = votacion.Id, Votacion = votacion };
        var criterio2 = new CriterioVotacion { Nombre = "Impacto",    Peso = 0.4m, VotacionId = votacion.Id, Votacion = votacion };
        votacion.Criterios = new List<CriterioVotacion> { criterio1, criterio2 };

        var proyecto = CrearProyecto((EstandarEvento)votacion.evento);
        ctx.Votaciones.Add(votacion);
        ctx.Projects.Add(proyecto);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = new EmitirVotoDto
        {
            VotacionId = votacion.Id,
            UsuarioId = 0,
            VotosProyectos = new List<VotoProyectoDto>
            {
                new()
                {
                    ProyectoId = proyecto.Id,
                    CriterioVotos = new Dictionary<int, decimal>
                    {
                        { criterio1.Id, 8m },
                        { criterio2.Id, 5m }
                    }
                }
            }
        };

        // Act
        var resultado = await service.EmitirVotoAsync(dto);

        // Assert
        Assert.True(resultado);
        var voto = ctx.Votes.First();
        Assert.Equal(7, voto.Valoracion); // 0.6*8 + 0.4*5 = 6.8 → round → 7
    }

    // ── Caminos de error — validación de estrategia ────────────────────────

    /// <summary>
    /// PE-04: La estrategia Binaria lanza excepción cuando el valor del voto
    /// no es 0 ni 1 (valor 2 en este caso).
    /// </summary>
    [Fact]
    public async Task Estrategia_Binaria_ValorInvalido_LanzaArgumentException()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(Estrategia_Binaria_ValorInvalido_LanzaArgumentException));
        var votacion = CrearVotacion(TipoVotacion.Binaria);
        var proyecto = CrearProyecto((EstandarEvento)votacion.evento);
        ctx.Votaciones.Add(votacion);
        ctx.Projects.Add(proyecto);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = new EmitirVotoDto
        {
            VotacionId = votacion.Id,
            UsuarioId = 0,
            VotosProyectos = new List<VotoProyectoDto>
            {
                new() { ProyectoId = proyecto.Id, Valoracion = 2 } // Inválido: no es 0 ni 1
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.EmitirVotoAsync(dto));

        Assert.Contains("0 o 1", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// PE-05: La estrategia Numérica lanza excepción cuando la valoración supera
    /// el valor máximo configurado en la votación.
    /// </summary>
    [Fact]
    public async Task Estrategia_Numerica_ValorSuperaMaximo_LanzaArgumentException()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(Estrategia_Numerica_ValorSuperaMaximo_LanzaArgumentException));
        var votacion = CrearVotacion(TipoVotacion.Numerica, valorMaximo: 5);
        var proyecto = CrearProyecto((EstandarEvento)votacion.evento);
        ctx.Votaciones.Add(votacion);
        ctx.Projects.Add(proyecto);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = new EmitirVotoDto
        {
            VotacionId = votacion.Id,
            UsuarioId = 0,
            VotosProyectos = new List<VotoProyectoDto>
            {
                new() { ProyectoId = proyecto.Id, Valoracion = 9 } // Supera máximo de 5
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.EmitirVotoAsync(dto));

        Assert.Contains("Máximo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// PE-06: La estrategia CriteriosPorPeso lanza excepción cuando no se
    /// proporcionan valoraciones por criterio (CriterioVotos nulo o vacío).
    /// </summary>
    [Fact]
    public async Task Estrategia_CriteriosPorPeso_SinCriterioVotos_LanzaArgumentException()
    {
        // Arrange
        using var ctx = CrearContexto(nameof(Estrategia_CriteriosPorPeso_SinCriterioVotos_LanzaArgumentException));
        var votacion = CrearVotacion(TipoVotacion.CriteriosPorPeso);
        var proyecto = CrearProyecto((EstandarEvento)votacion.evento);
        ctx.Votaciones.Add(votacion);
        ctx.Projects.Add(proyecto);
        await ctx.SaveChangesAsync();

        var service = new VoteService(new EntityFrameworkDAL(ctx));
        var dto = new EmitirVotoDto
        {
            VotacionId = votacion.Id,
            UsuarioId = 0,
            VotosProyectos = new List<VotoProyectoDto>
            {
                new() { ProyectoId = proyecto.Id, CriterioVotos = null } // Sin criterios
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.EmitirVotoAsync(dto));

        Assert.Contains("criterios", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
