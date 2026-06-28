using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTO;
using Domain.Entitites;
using Shared.Enums;

namespace BusinessLogic.Mappers
{
    public static class VotacionMapper
    {
        public static VotacionDto ToDto(this Votacion v)
        {
            if (v == null) return null;

            return new VotacionDto
            {
                Id = v.Id,
                Nombre = v.Nombre,
                Estado = v.Estado.ToString(),
                EstadoComentarios = v.EstadoComentarios,
                Descripcion = v.Descripcion,
                FechaInicio = v.FechaInicio,
                FechaFin = v.FechaFin,
                MaxVotesPerVoter = v.MaxVotesPerVoter,
                EstaActiva = v.EstaActiva,
                EsPopular = v.EsVotacionPopular,
                Duracion = v.Duracion
                ,
                TipoVotacion = v.TipoVotacion,
                ValorMaximoNumerico = v.ValorMaximoNumerico,
                Criterios = v.Criterios?.Select(c => new CriterioVotacionDto { Id = c.Id, Nombre = c.Nombre, Descripcion = c.Descripcion, Peso = c.Peso }).ToList() ?? new List<CriterioVotacionDto>(),
                Premio = v.Premio == null ? null : new PremioDTO
                {
                    Id = v.Premio.Id,
                    Emoji = v.Premio.Emoji,
                    Nombre = v.Premio.Nombre,
                    ProyectoGanadorId = v.Premio.ProyectoGanadorId
                }
            };
        }

        public static JuradoSeleccionadoDto ToJuradoSeleccionadoDto(this Jurado jurado)
        {
            if (jurado == null) return null;

            return new JuradoSeleccionadoDto
            {
                Id = jurado.Id,
                Nombre = jurado.Nombre,
                Dni = jurado.Dni
            };
        }
    }
}
