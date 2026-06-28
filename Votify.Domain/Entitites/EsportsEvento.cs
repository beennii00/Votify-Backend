using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitites
{
    public class EsportsEvento : Evento
    {
        public string Juego { get; set; }
        public string Plataforma { get; set; }

        protected EsportsEvento() { }

        public EsportsEvento(string nombre, string descripcion, DateTime fechaInicio, DateTime fechaFin, string juego, string plataforma)
            : base(nombre, descripcion, fechaInicio, fechaFin)
        {
            Juego = juego;
            Plataforma = plataforma;
        }
    }
}
