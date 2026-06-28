using Domain.Entitites;
using Shared.DTO;
using Shared.Enums;
using System.Text.Json;

namespace BusinessLogic.Strategies
{
    public class CriteriosPorPesoStrategy : IVoteProcessorStrategy
    {
        public Voto ProcesarVoto(Votacion votacion, Proyecto proyecto, VotoProyectoDto vp, Usuario? usuario)
        {
            if (vp.CriterioVotos == null || vp.CriterioVotos.Count == 0)
                throw new ArgumentException("Las votaciones por criterios requieren valores para cada criterio.");

            var criteriaMap = votacion.Criterios?.ToDictionary(c => c.Id, c => c.Peso) ?? new Dictionary<int, decimal>();
            decimal weightedSum = 0M;
            foreach (var kv in vp.CriterioVotos)
            {
                if (!criteriaMap.TryGetValue(kv.Key, out var peso))
                    throw new ArgumentException($"Criterio inválido: {kv.Key}");
                weightedSum += peso * kv.Value;
            }

            int valoracion = (int)Math.Round((double)weightedSum);
            var voto = new Voto(DateTime.UtcNow, usuario, votacion, proyecto, valoracion);
            voto.CriterioValoracionesJson = JsonSerializer.Serialize(vp.CriterioVotos);

            // Comentarios para votaciones por criterios: pueden venir por criterio o un comentario general.
            if (votacion.EstadoComentarios == EstadoComentarios.Obligatorios)
            {
                bool hasGeneral = !string.IsNullOrWhiteSpace(vp.Comentario);
                bool hasAllCriteria = votacion.Criterios != null && votacion.Criterios.All(c => vp.ComentariosPorCriterio != null && vp.ComentariosPorCriterio.TryGetValue(c.Id, out var txt) && !string.IsNullOrWhiteSpace(txt));
                if (!hasGeneral && !hasAllCriteria)
                    throw new ArgumentException("Los comentarios por criterio (o un comentario general) son obligatorios para evaluar proyectos en esta votación.");
            }

            if (votacion.EstadoComentarios != EstadoComentarios.Desactivados)
            {
                if (!string.IsNullOrWhiteSpace(vp.Comentario))
                {
                    voto.Comentarios.Add(new Comentario(vp.Comentario, DateTime.UtcNow, voto, null));
                }

                if (vp.ComentariosPorCriterio != null)
                {
                    foreach (var kv in vp.ComentariosPorCriterio)
                    {
                        if (string.IsNullOrWhiteSpace(kv.Value)) continue;
                        voto.Comentarios.Add(new Comentario(kv.Value, DateTime.UtcNow, voto, kv.Key));
                    }
                }
            }

            return voto;
        }
    }
}
