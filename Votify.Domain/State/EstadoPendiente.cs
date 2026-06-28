using System;
using Domain.Entitites;
using Shared.Enums;

namespace Domain.State
{
    public class EstadoPendiente : IEstadoVotacion
    {
        public EstadoVotacion CodigoEstado => EstadoVotacion.Pendiente;

        public void Iniciar(Votacion ctx)
        {
            ctx.FechaInicio = DateTime.UtcNow;
            ctx.SetEstado(new EstadoActiva());
        }

        public void Pausar(Votacion ctx) => throw new InvalidOperationException("No se puede pausar una votación pendiente.");
        public void Reanudar(Votacion ctx) => throw new InvalidOperationException("No se puede reanudar una votación pendiente.");
        public void Cerrar(Votacion ctx) => throw new InvalidOperationException("No se puede cerrar una votación pendiente.");
    }
}
