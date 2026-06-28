using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services;
using Domain.Entitites;
using Microsoft.EntityFrameworkCore;
using Persistance.Data;
using Persistance.Repositories;
using Shared.DTO;
using Votify.BusinessLogic.Services;
using Xunit;
using Domain.State;

namespace TestProject_xUnit;

public class AsignarJuradosAsyncTests
{
    private static AppDbContext CrearContexto(string nombre) =>
        new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(nombre)
                .Options);

    private static Administrador CrearAdmin() => new Administrador("AdminTest", "11111111A", "pass");
    private static Jurado CrearJurado(int index) => new Jurado($"Jurado{index}", $"2222222{index}A", "pass");
    private static EstandarEvento CrearEvento() => new EstandarEvento("Evento", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
    
    private static Votacion CrearVotacion()
    {
        var vot = new Votacion
        {
            Nombre = "Votación Test",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddHours(2),
            evento = CrearEvento()
        };
        vot.SetEstado(new EstadoActiva());
        return vot;
    }

    [Fact]
    public async Task AsignarJuradosAsync_AsignaJuradosCorrectamente_Limpiamente()
    {
        // Arrange
        var dbName = nameof(AsignarJuradosAsync_AsignaJuradosCorrectamente_Limpiamente);
        using var ctx = CrearContexto(dbName);
        
        var admin = CrearAdmin();
        var j1 = CrearJurado(1);
        var j2 = CrearJurado(2);
        var j3 = CrearJurado(3);
        var votacion = CrearVotacion();
        
        // Add an existing juror to prove it clears first
        votacion.JuradosAsignados.Add(j3);

        ctx.Users.AddRange(admin, j1, j2, j3);
        ctx.Votaciones.Add(votacion);
        await ctx.SaveChangesAsync();

        var service = new VotacionService(new EntityFrameworkDAL(ctx), null!);
        
        var dto = new AsignarJuradosDto
        {
            AdministradorId = admin.Id,
            VotacionId = votacion.Id,
            JuradoIds = new List<int> { j1.Id, j2.Id }
        };

        // Act
        var result = await service.AsignarJuradosAsync(dto);

        // Assert
        using var assertCtx = CrearContexto(dbName);
        var votacionDb = await assertCtx.Votaciones.Include(v => v.JuradosAsignados).FirstAsync(v => v.Id == votacion.Id);
        
        Assert.True(result);
        Assert.Equal(2, votacionDb.JuradosAsignados.Count);
        Assert.Contains(votacionDb.JuradosAsignados, j => j.Id == j1.Id);
        Assert.Contains(votacionDb.JuradosAsignados, j => j.Id == j2.Id);
        Assert.DoesNotContain(votacionDb.JuradosAsignados, j => j.Id == j3.Id);
    }
}
