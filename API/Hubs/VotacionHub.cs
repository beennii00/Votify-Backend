using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Hubs
{
    // 1. Interfaz estricta del Patrón Sujeto
    public interface ISujetoVotacion
    {
        Task SuscribirObservador(string connectionId, int idVotacion);
        Task CancelarSuscripcion(string connectionId, int idVotacion);
        Task CancelarSuscripcionGlobal(string connectionId); // Para cuando cierran el navegador de golpe
        Task NotificarCambioVotos(int idVotacion); // <-- ¡Aquí está Notificar!
    }

    // 2. Sujeto Concreto: Mantiene el diccionario de estado y ejecuta las notificaciones
    public class GestorVotosRealTime : ISujetoVotacion
    {
        private readonly IHubContext<VotacionHub> _hubContext;
        
        // estadoSujeto: Diccionario = [IdVotacion] -> [Lista de ConnectionIds (Observadores)]
        // Usamos ConcurrentDictionary por seguridad en hilos web
        private readonly ConcurrentDictionary<int, List<string>> _observadoresPorVotacion = new();

        public GestorVotosRealTime(IHubContext<VotacionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Equivalente a Attach(o : Observador)
        public Task SuscribirObservador(string connectionId, int idVotacion)
        {
            var observadores = _observadoresPorVotacion.GetOrAdd(idVotacion, new List<string>());
            lock (observadores)
            {
                if (!observadores.Contains(connectionId))
                {
                    observadores.Add(connectionId);
                }
            }
            return Task.CompletedTask;
        }

        // Equivalente a Dettach(o : Observador)
        public Task CancelarSuscripcion(string connectionId, int idVotacion)
        {
            if (_observadoresPorVotacion.TryGetValue(idVotacion, out var observadores))
            {
                lock (observadores)
                {
                    observadores.Remove(connectionId);
                }
            }
            return Task.CompletedTask;
        }

        // Limpieza por si el WebSocket se corta abruptamente
        public Task CancelarSuscripcionGlobal(string connectionId)
        {
            foreach (var observadores in _observadoresPorVotacion.Values)
            {
                lock (observadores)
                {
                    observadores.Remove(connectionId);
                }
            }
            return Task.CompletedTask;
        }

        // Equivalente a Notificar()
        public async Task NotificarCambioVotos(int idVotacion)
        {
            if (_observadoresPorVotacion.TryGetValue(idVotacion, out var observadores))
            {
                List<string> observadoresCopia;
                lock (observadores)
                {
                    observadoresCopia = observadores.ToList();
                }

                // para cada o en observadores -> o->Actualizar()
                foreach (var connectionId in observadoresCopia)
                {
                    // Se envía el mensaje directamente al observador (Cliente físico)
                    await _hubContext.Clients.Client(connectionId).SendAsync("ActualizarDatosVotacion");
                }
            }
        }
    }

    // 3. El Hub ya No es el sujeto, es solo el Endpoint de red que recibe la conexión WASM
    public class VotacionHub : Hub
    {
        private readonly ISujetoVotacion _sujeto;

        public VotacionHub(ISujetoVotacion sujeto)
        {
            _sujeto = sujeto;
        }

        public async Task SuscribirObservador(int idVotacion)
        {
            // El cliente llega por red, el Hub avisa al Sujeto
            await _sujeto.SuscribirObservador(Context.ConnectionId, idVotacion);
        }

        public async Task CancelarSuscripcion(int idVotacion)
        {
            await _sujeto.CancelarSuscripcion(Context.ConnectionId, idVotacion);
        }

        // Se ejecuta automáticamente si el usuario cierra el navegador o pierde internet
        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            await _sujeto.CancelarSuscripcionGlobal(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
