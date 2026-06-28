using System;
using Domain.Entitites;
using Shared.Enums;

namespace Domain.State
{
    public class EstadoPausada : IEstadoVotacion
    {
        public EstadoVotacion CodigoEstado => EstadoVotacion.Pausada;

        public void Iniciar(Votacion ctx) => throw new InvalidOperationException("La votación ya ha sido iniciada y actualmente está pausada.");
        
        public void Pausar(Votacion ctx) => throw new InvalidOperationException("La votación ya está pausada.");

        public void Reanudar(Votacion ctx)
        {
            ctx.SetEstado(new EstadoActiva());
        }
        
        public void Cerrar(Votacion ctx)
        {
            ctx.FechaFin = DateTime.UtcNow;
            ctx.SetEstado(new EstadoCerrada());
        }
    }
}
