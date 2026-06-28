using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entitites;

namespace Domain.Factory
{
    public abstract class EventoCreator
    {
        public abstract Evento CreateEvento(string nombre, string descripcion, DateTime fechaInicio, DateTime fechaFin);
    }
}
