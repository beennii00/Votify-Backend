using System;
using System.Collections.Generic;

namespace Shared.DTO
{
    public class EmitirVotoDto
    {
        public int VotacionId { get; set; }
        public int UsuarioId { get; set; }
        public List<VotoProyectoDto> VotosProyectos { get; set; } = new();
    }

    public class VotoProyectoDto
    {
        public int ProyectoId { get; set; }
        public int? Valoracion { get; set; } // Para votación Binaria o Numérica
        public Dictionary<int, decimal>? CriterioVotos { get; set; } // Diccionario con CriterioId y su valoracion dada
        public string? Comentario { get; set; }
        public Dictionary<int, string>? ComentariosPorCriterio { get; set; }
    }
}
