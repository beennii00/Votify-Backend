using Domain.Entitites;
using Shared.DTO;
using Shared.Enums;

namespace BusinessLogic.Strategies
{
    public class BinariaStrategy : IVoteProcessorStrategy
    {
        public Voto ProcesarVoto(Votacion votacion, Proyecto proyecto, VotoProyectoDto votoDto, Usuario? usuario)
        {
            // ── Validación específica de votación binaria ────────────────────────
            if (!votoDto.Valoracion.HasValue)
                throw new ArgumentException("Se requiere una valoración binaria (0/1). Si corresponde, marca la opción.");

            if (!(votoDto.Valoracion.Value == 0 || votoDto.Valoracion.Value == 1))
                throw new ArgumentException("La valoración binaria debe ser 0 o 1.");

            // ── Construcción del voto ────────────────────────────────────────────
            var voto = new Voto(DateTime.UtcNow, usuario, votacion, proyecto, votoDto.Valoracion.Value);

            // ── Comentarios ──────────────────────────────────────────────────────
            AgregarComentarios(voto, votacion, votoDto);

            return voto;
        }

        private static void AgregarComentarios(Voto voto, Votacion votacion, VotoProyectoDto votoDto)
        {
            if (votacion.EstadoComentarios == EstadoComentarios.Desactivados) return;

            if (!string.IsNullOrWhiteSpace(votoDto.Comentario))
                voto.Comentarios.Add(new Comentario(votoDto.Comentario, DateTime.UtcNow, voto, null));

            if (votoDto.ComentariosPorCriterio != null)
            {
                foreach (var kv in votoDto.ComentariosPorCriterio)
                {
                    if (!string.IsNullOrWhiteSpace(kv.Value))
                        voto.Comentarios.Add(new Comentario(kv.Value, DateTime.UtcNow, voto, kv.Key));
                }
            }
        }
    }
}
