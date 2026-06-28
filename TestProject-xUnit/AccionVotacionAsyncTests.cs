using BusinessLogic.Interfaces;
using BusinessLogic.Services;
using Domain.Entitites;
using Microsoft.EntityFrameworkCore;
using Persistance.Data;
using Persistance.Repositories;
using Shared.DTO;
using Shared.Enums;
using Domain.State;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Votify.BusinessLogic.Services; // Ensure using VotacionService is included

namespace TestProject_xUnit
{
    // Fake IA Service para evitar dependencia de Moq
    public class FakeIACommentSynthesisService : IIACommentSynthesisService
    {
        public Task<string> ResumirComentariosAsync(int proyectoId, List<string> comentarios)
        {
            return Task.FromResult("Resumen simulado por IA");
        }
    }

    public class AccionVotacionAsyncTests
    {
        private static AppDbContext CrearContexto(string nombre) =>
            new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(nombre)
                    .Options);

        private static EstandarEvento CrearEventoConVotacion(int eventoId, int votacionId, AppDbContext ctx)
        {
            var evento = new EstandarEvento("Evento Test", "Desc", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10))
            {
                Id = eventoId
            };
            var votacion = new Votacion
            {
                Id = votacionId,
                Nombre = "Votaci�n de prueba",
                evento = evento
            };
            votacion.SetEstado(new EstadoPendiente());
            evento.votacion.Add(votacion);

            ctx.Events.Add(evento);
            ctx.Votaciones.Add(votacion);
            return evento;
        }

        [Fact]
        public async Task AccionVotacion_Cerrar_CambiaEstadoACerrada()
        {
            // Arrange
            using var ctx = CrearContexto(nameof(AccionVotacion_Cerrar_CambiaEstadoACerrada));
            var evento = CrearEventoConVotacion(1, 1, ctx);
            await ctx.SaveChangesAsync();

            var dal = new EntityFrameworkDAL(ctx);
            var fakeIaService = new FakeIACommentSynthesisService();
            var service = new VotacionService(dal, fakeIaService);

            // Act
            var result = await service.AccionVotacionAsync(1, 1, "cerrar");

            // Assert
            Assert.NotNull(result);
            
            var votacionEnBd = await ctx.Votaciones.FindAsync(1);
            Assert.NotNull(votacionEnBd);
            Assert.Equal(EstadoVotacion.Cerrada, votacionEnBd.Estado);
            // La fecha fin ahora deber�a estar establecida al momento del cierre
            Assert.True((DateTime.UtcNow - votacionEnBd.FechaFin).Duration() < TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task AccionVotacion_Iniciar_CambiaEstadoAActiva()
        {
            // Arrange
            using var ctx = CrearContexto(nameof(AccionVotacion_Iniciar_CambiaEstadoAActiva));
            var evento = CrearEventoConVotacion(1, 2, ctx);
            await ctx.SaveChangesAsync();

            var dal = new EntityFrameworkDAL(ctx);
            var fakeIaService = new FakeIACommentSynthesisService();
            var service = new VotacionService(dal, fakeIaService);

            // Act
            var result = await service.AccionVotacionAsync(1, 2, "iniciar");

            // Assert
            var votacionEnBd = await ctx.Votaciones.FindAsync(2);
            Assert.Equal(EstadoVotacion.Activa, votacionEnBd!.Estado);
            Assert.True((DateTime.UtcNow - votacionEnBd.FechaInicio).Duration() < TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task AsignarJuradosAsync_AsignaJuradosCorrectamente()
        {
            // Arrange
            using var ctx = CrearContexto(nameof(AsignarJuradosAsync_AsignaJuradosCorrectamente));
            var evento = CrearEventoConVotacion(10, 10, ctx);
            
            var admin = new Administrador("Admin", "12345678A", "hash123") { Id = 1 };
            ctx.Set<Usuario>().Add(admin);

            var jurado1 = new Jurado("Jurado 1", "22345678A", "hash123") { Id = 2 };
            var jurado2 = new Jurado("Jurado 2", "32345678A", "hash123") { Id = 3 };
            ctx.Set<Usuario>().Add(jurado1);
            ctx.Set<Usuario>().Add(jurado2);

            await ctx.SaveChangesAsync();

            var dal = new EntityFrameworkDAL(ctx);
            var fakeIaService = new FakeIACommentSynthesisService();
            var service = new VotacionService(dal, fakeIaService);

            var dto = new AsignarJuradosDto { AdministradorId = 1, VotacionId = 10, JuradoIds = new List<int> { 2, 3 } };

            // Act
            var result = await service.AsignarJuradosAsync(dto);

            // Assert
            Assert.True(result);
            var votacionEnBd = await ctx.Votaciones
                .Include(v => v.JuradosAsignados)
                .FirstOrDefaultAsync(v => v.Id == 10);
            
            Assert.NotNull(votacionEnBd);
            Assert.Equal(2, votacionEnBd.JuradosAsignados.Count);
            Assert.Contains(votacionEnBd.JuradosAsignados, j => j.Id == 2);
            Assert.Contains(votacionEnBd.JuradosAsignados, j => j.Id == 3);
        }

        [Fact]
        public async Task AccionVotacionAsync_AlCerrar_AsignaGanadorAutomaticamente()
        {
            // Arrange
            using var ctx = CrearContexto(nameof(AccionVotacionAsync_AlCerrar_AsignaGanadorAutomaticamente));
            var evento = CrearEventoConVotacion(20, 20, ctx);
            
            var proyecto = new ProyectoEstandar("Proyecto Ganador", "Desc", evento) { Id = 100 };
            ctx.Set<Proyecto>().Add(proyecto);

            var votacion = await ctx.Votaciones.FindAsync(20);
            votacion!.Premio = new Premio { Id = 1, Nombre = "Premio Test" }; // VotacionId will be set explicitly when added or managed by ef implicitly by navigation
            votacion.Premio.Votacion = votacion;
            
            var usuario = new Concursante("Concursante 1", "44445555A", "hash");
            ctx.Set<Usuario>().Add(usuario);

            var voto = new Voto { 
                Votacion = votacion, 
                Proy = proyecto, 
                NombreUsuario = usuario,
                Valoracion = 5
            };
            ctx.Set<Voto>().Add(voto);

            await ctx.SaveChangesAsync();

            var dal = new EntityFrameworkDAL(ctx);
            var fakeIaService = new FakeIACommentSynthesisService();
            var service = new VotacionService(dal, fakeIaService);

            // Act
            var result = await service.AccionVotacionAsync(20, 20, "cerrar");

            // Assert
            var votacionEnBd = await ctx.Votaciones
                .Include(v => v.Premio)
                .FirstOrDefaultAsync(v => v.Id == 20);

            Assert.Equal(EstadoVotacion.Cerrada, votacionEnBd!.Estado);
            Assert.NotNull(votacionEnBd.Premio);
            Assert.Equal(100, votacionEnBd.Premio.ProyectoGanadorId);
        }
    }
}
