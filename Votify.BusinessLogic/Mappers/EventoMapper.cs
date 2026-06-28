using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTO;
using Domain.Entitites;
using System.Linq;
using System;

namespace BusinessLogic.Mappers
{
    public static class EventoMapper
    {
        public static EventoDTO ToDto(this Evento evento)
        {
            if (evento == null) return null;

            var votacionesDto = evento.votacion?.Select(v => v.ToDto()).ToList();

            return evento switch
            {
                EsportsEvento esports => new ESportsEventoDto
                {
                    Id = esports.Id,
                    Nombre = esports.Nombre,
                    Descripcion = esports.Descripcion,
                    FechaInicio = esports.FechaInicio,
                    FechaFin = esports.FechaFin,
                    Juego = esports.Juego,
                    Plataforma = esports.Plataforma,
                    Votacion = votacionesDto
                },
                EstandarEvento estandar => new EstandarEventoDto
                {
                    Id = estandar.Id,
                    Nombre = estandar.Nombre,
                    Descripcion = estandar.Descripcion,
                    FechaInicio = estandar.FechaInicio,
                    FechaFin = estandar.FechaFin,
                    Votacion = votacionesDto
                },
                _ => throw new InvalidOperationException("Tipo de evento desconocido.")
            };
        }
    }
}
