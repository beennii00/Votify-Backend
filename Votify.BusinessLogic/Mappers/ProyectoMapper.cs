using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTO;
using Domain.Entitites;

namespace BusinessLogic.Mappers
{
    public static class ProyectoMapper
    {
        public static ProyectoDTO ToDto(this Proyecto p)
        {
            if (p == null) return null;

            return p switch
            {
                ProyectoEstandar estandar => new EstandarProyectoDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    EventoId = p.Evento?.Id ?? 0
                },

                _ => throw new InvalidOperationException("Tipo de evento desconocido.")
            };
        }
    }
}
