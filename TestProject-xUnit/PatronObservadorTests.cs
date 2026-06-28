using API.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace TestProject_xUnit;

// ══════════════════════════════════════════════════════════════════
//  Fake manual de IHubContext para poder testear NotificarCambioVotos
//  sin depender de librerías de mocking (mismo enfoque que InMemoryDb).
//  Registra qué mensajes se enviaron a cada ConnectionId.
// ══════════════════════════════════════════════════════════════════

/// <summary>Registra las llamadas a SendAsync para poder verificarlas.</summary>
public class FakeClientProxy : IClientProxy
{
    public List<string> MensajesRecibidos { get; } = new();

    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
    {
        MensajesRecibidos.Add(method);
        return Task.CompletedTask;
    }
}

/// <summary>Devuelve un FakeClientProxy por cada ConnectionId solicitado.</summary>
public class FakeHubClients : IHubClients
{
    public ConcurrentDictionary<string, FakeClientProxy> Clientes { get; } = new();

    public IClientProxy Client(string connectionId)
    {
        return Clientes.GetOrAdd(connectionId, _ => new FakeClientProxy());
    }

    // Métodos que no usamos pero la interfaz exige:
    public IClientProxy All => throw new NotImplementedException();
    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => throw new NotImplementedException();
    public IClientProxy Group(string groupName) => throw new NotImplementedException();
    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => throw new NotImplementedException();
    public IClientProxy Groups(IReadOnlyList<string> groupNames) => throw new NotImplementedException();
    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => throw new NotImplementedException();
    public IClientProxy User(string userId) => throw new NotImplementedException();
    public IClientProxy Users(IReadOnlyList<string> userIds) => throw new NotImplementedException();
}

/// <summary>Implementación fake mínima de IHubContext para tests.</summary>
public class FakeHubContext : IHubContext<VotacionHub>
{
    public FakeHubClients FakeClients { get; } = new();
    public IHubClients Clients => FakeClients;
    public IGroupManager Groups => throw new NotImplementedException();
}


// ══════════════════════════════════════════════════════════════════
//  Tests del Patrón Observador (GestorVotosRealTime)
// ══════════════════════════════════════════════════════════════════

public class PatronObservadorTests
{
    // ── Helpers ──────────────────────────────────────────────────

    private static (GestorVotosRealTime sujeto, FakeHubContext fakeHub) CrearSujeto()
    {
        var fakeHub = new FakeHubContext();
        var sujeto = new GestorVotosRealTime(fakeHub);
        return (sujeto, fakeHub);
    }

    // ── Attach (SuscribirObservador) ────────────────────────────

    [Fact]
    public async Task Attach_SuscribirObservador_SeRegistraCorrectamente()
    {
        // Arrange
        var (sujeto, _) = CrearSujeto();

        // Act
        await sujeto.SuscribirObservador("conn-1", idVotacion: 10);
        await sujeto.SuscribirObservador("conn-2", idVotacion: 10);

        // Assert — Notificamos y verificamos que llegó a ambos
        await sujeto.NotificarCambioVotos(idVotacion: 10);
        // Si recibieron el mensaje, es que estaban suscritos
        // (se verifica indirectamente en los tests de Notify)
    }

    [Fact]
    public async Task Attach_MismoObservadorDosVeces_NoSeDuplica()
    {
        // Arrange
        var (sujeto, fakeHub) = CrearSujeto();

        // Act — Suscribir el mismo ConnectionId dos veces
        await sujeto.SuscribirObservador("conn-1", idVotacion: 10);
        await sujeto.SuscribirObservador("conn-1", idVotacion: 10);

        // Assert — Al notificar, solo debe recibir UNA señal, no dos
        await sujeto.NotificarCambioVotos(idVotacion: 10);

        var clientProxy = fakeHub.FakeClients.Clientes["conn-1"];
        Assert.Single(clientProxy.MensajesRecibidos);
    }

    // ── Detach (CancelarSuscripcion) ────────────────────────────

    [Fact]
    public async Task Detach_ObservadorDesuscrito_NoRecibeNotificacion()
    {
        // Arrange
        var (sujeto, fakeHub) = CrearSujeto();
        await sujeto.SuscribirObservador("conn-1", idVotacion: 10);
        await sujeto.SuscribirObservador("conn-2", idVotacion: 10);

        // Act — Desuscribir conn-1
        await sujeto.CancelarSuscripcion("conn-1", idVotacion: 10);
        await sujeto.NotificarCambioVotos(idVotacion: 10);

        // Assert — conn-1 NO debe recibir, conn-2 SÍ
        Assert.False(fakeHub.FakeClients.Clientes.ContainsKey("conn-1"));
        Assert.Single(fakeHub.FakeClients.Clientes["conn-2"].MensajesRecibidos);
    }

    // ── Detach Global (CancelarSuscripcionGlobal) ───────────────

    [Fact]
    public async Task DetachGlobal_ObservadorEnVariasVotaciones_SeEliminaDeTodas()
    {
        // Arrange — conn-1 suscrito a votación 10 y 20
        var (sujeto, fakeHub) = CrearSujeto();
        await sujeto.SuscribirObservador("conn-1", idVotacion: 10);
        await sujeto.SuscribirObservador("conn-1", idVotacion: 20);

        // Act — Simular que cierra el navegador → Detach global
        await sujeto.CancelarSuscripcionGlobal("conn-1");

        // Assert — No debe recibir notificación en ninguna votación
        await sujeto.NotificarCambioVotos(idVotacion: 10);
        await sujeto.NotificarCambioVotos(idVotacion: 20);

        Assert.False(fakeHub.FakeClients.Clientes.ContainsKey("conn-1"));
    }

    // ── Notify (NotificarCambioVotos) ───────────────────────────

    [Fact]
    public async Task Notify_EnviaActualizarDatosVotacion_ATodosLosSuscritos()
    {
        // Arrange
        var (sujeto, fakeHub) = CrearSujeto();
        await sujeto.SuscribirObservador("conn-1", idVotacion: 10);
        await sujeto.SuscribirObservador("conn-2", idVotacion: 10);
        await sujeto.SuscribirObservador("conn-3", idVotacion: 10);

        // Act
        await sujeto.NotificarCambioVotos(idVotacion: 10);

        // Assert — Los 3 deben haber recibido "ActualizarDatosVotacion"
        foreach (var connId in new[] { "conn-1", "conn-2", "conn-3" })
        {
            var proxy = fakeHub.FakeClients.Clientes[connId];
            Assert.Single(proxy.MensajesRecibidos);
            Assert.Equal("ActualizarDatosVotacion", proxy.MensajesRecibidos[0]);
        }
    }

    [Fact]
    public async Task Notify_SoloNotificaVotacionCorrecta_NoAfectaOtras()
    {
        // Arrange — conn-1 en votación 10, conn-2 en votación 20
        var (sujeto, fakeHub) = CrearSujeto();
        await sujeto.SuscribirObservador("conn-1", idVotacion: 10);
        await sujeto.SuscribirObservador("conn-2", idVotacion: 20);

        // Act — Solo notificar votación 10
        await sujeto.NotificarCambioVotos(idVotacion: 10);

        // Assert — conn-1 recibe, conn-2 NO
        Assert.Single(fakeHub.FakeClients.Clientes["conn-1"].MensajesRecibidos);
        Assert.False(fakeHub.FakeClients.Clientes.ContainsKey("conn-2"));
    }

    [Fact]
    public async Task Notify_VotacionSinObservadores_NoLanzaError()
    {
        // Arrange
        var (sujeto, _) = CrearSujeto();

        // Act & Assert — No debe lanzar excepción
        await sujeto.NotificarCambioVotos(idVotacion: 999);
    }
}
