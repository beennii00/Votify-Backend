using System;

namespace Domain.Entitites
{
    public class ProyectoEstandar : Proyecto
    {
        protected ProyectoEstandar() { }

        public ProyectoEstandar(string nombre, string descripcion, Evento evento)
            : base(nombre, descripcion, evento)
        {
        }
    }
}