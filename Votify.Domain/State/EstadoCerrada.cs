using System;
using Domain.Entitites;
using Shared.Enums;

namespace Domain.State
{
    public class EstadoCerrada : IEstadoVotacion
    {
        public EstadoVotacion CodigoEstado => EstadoVotacion.Cerrada;

        public void Iniciar(Votacion ctx) => throw new InvalidOperationException("No se puede iniciar una votación cerrada.");
        
        public void Pausar(Votacion ctx) => throw new InvalidOperationException("No se puede pausar una votación cerrada.");

        public void Reanudar(Votacion ctx) => throw new InvalidOperationException("No se puede reanudar una votación cerrada.");
        
        public void Cerrar(Votacion ctx) => throw new InvalidOperationException("La votación ya está cerrada.");
    }
}
