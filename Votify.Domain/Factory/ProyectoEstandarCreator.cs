using System;
using Domain.Entitites;

namespace Domain.Factory
{
    public class ProyectoEstandarCreator : ProyectoCreator
    {
        public override Proyecto CreateProyecto(string nombre, string descripcion, Evento evento)
        {
            return new ProyectoEstandar(nombre, descripcion, evento);
        }
    }
}