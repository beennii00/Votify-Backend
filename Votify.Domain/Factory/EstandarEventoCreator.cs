using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entitites;

namespace Domain.Factory
{
    public class EstandarEventoCreator : EventoCreator
    {
        public override Evento CreateEvento(string nombre, string descripcion, 
            DateTime fechaInicio, DateTime fechaFin)
        {
            return new EstandarEvento(nombre, descripcion, fechaInicio, fechaFin);
        }
    }
}
