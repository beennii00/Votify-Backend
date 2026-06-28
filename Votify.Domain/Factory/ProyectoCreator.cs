using System;
using Domain.Entitites;

namespace Domain.Factory
{
    public abstract class ProyectoCreator
    {
        public abstract Proyecto CreateProyecto(string nombre, string descripcion, Evento evento);
    }
}