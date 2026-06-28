using Domain.Entitites;
using Shared.DTO;
using Shared.Enums;

namespace BusinessLogic.Strategies
{
    public class NumericaStrategy : IVoteProcessorStrategy
    {
        public Voto ProcesarVoto(Votacion votacion, Proyecto proyecto, VotoProyectoDto votoDto, Usuario? usuario)
        {
            // ── Validación específica de votación numérica ───────────────────────
            if (!votoDto.Valoracion.HasValue || votoDto.Valoracion.Value <= 0)
                throw new ArgumentException("Se requiere una valoración numérica válida.");

            if (votacion.ValorMaximoNumerico.HasValue && votoDto.Valoracion.Value > votacion.ValorMaximoNumerico.Value)
                throw new ArgumentException($"Valoración numérica no válida. Máximo: {votacion.ValorMaximoNumerico.Value}");

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
