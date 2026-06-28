using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitites
{
    public class EstandarEvento : Evento
    {
        protected EstandarEvento() { }

        public EstandarEvento(string nombre, string descripcion, DateTime fechaInicio, DateTime fechaFin)
            : base(nombre, descripcion, fechaInicio, fechaFin)
        {

        }
    }
}
