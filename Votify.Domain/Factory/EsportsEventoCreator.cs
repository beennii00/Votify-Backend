using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entitites;

namespace Domain.Factory
{
    public class EsportsEventoCreator : EventoCreator
    {
        private readonly string _juego;
        private readonly string _plataforma;

        public EsportsEventoCreator(string juego, string plataforma)
        {
            _juego = juego;
            _plataforma = plataforma;
        }


        public override Evento CreateEvento(string nombre, string descripcion, DateTime fechaInicio, DateTime fechaFin)
        {
            return new EsportsEvento(nombre, descripcion, fechaInicio, fechaFin, _juego, _plataforma);
        }
    }
}
