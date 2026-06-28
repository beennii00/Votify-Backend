using System;
using Domain.Entitites;
using Shared.Enums;

namespace Domain.State
{
    public interface IEstadoVotacion
    {
        EstadoVotacion CodigoEstado { get; }
        void Iniciar(Votacion contexto);
        void Pausar(Votacion contexto);
        void Reanudar(Votacion contexto);
        void Cerrar(Votacion contexto);
    }
}
