using System;
using Domain.Entitites;
using Shared.Enums;

namespace Domain.State
{
    public class EstadoActiva : IEstadoVotacion
    {
        public EstadoVotacion CodigoEstado => EstadoVotacion.Activa;

        public void Iniciar(Votacion ctx) => throw new InvalidOperationException("La votación ya está activa.");
        
        public void Pausar(Votacion ctx)
        {
            ctx.SetEstado(new EstadoPausada());
        }

        public void Reanudar(Votacion ctx) => throw new InvalidOperationException("La votación ya está activa.");
        
        public void Cerrar(Votacion ctx)
        {
            ctx.FechaFin = DateTime.UtcNow;
            ctx.SetEstado(new EstadoCerrada());
        }
    }
}
