using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Shared.Enums;

namespace Shared.DTO
{
    public class PremioDto
    {
        public int Id { get; set; }
        public string Emoji { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int? ProyectoGanadorId { get; set; }
        public string? ProyectoGanadorNombre { get; set; }
        public List<UsuarioBuscadorDto> ProyectoGanadorConcursantes { get; set; } = new();
    }

}


namespace Shared.DTO
{
    public class DetalleVotacionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public EstadoComentarios EstadoComentarios { get; set; }
        public int MaxVotesPerVoter { get; set; }
        public bool EstaActiva { get; set; }
        public bool EsVotacionPopular { get; set; }
        public string? CodigoAcceso { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int VotantesUnicos { get; set; }
        public int TotalJurados { get; set; }
        public int ParticipacionPorcentaje { get; set; }
        public int ProyectosEnCarrera { get; set; }
        public string LiderProvisional { get; set; }
        public int LiderVotos { get; set; }
        public int LiderPorcentaje { get; set; }
        public List<UsuarioBuscadorDto> Jurados { get; set; } = new();
        public List<ProyectoResultadoDto> ResultadosProvisionales { get; set; } = new();
        public List<ComentarioAnonimoDto> Comentarios { get; set; } = new();
        public bool EsPopular { get; set; }
        public List<string> DnisPermitidos { get; set; } = new();
        // Template fields
        public TipoVotacion TipoVotacion { get; set; } = TipoVotacion.Numerica;
        public int? ValorMaximoNumerico { get; set; }
        public List<CriterioVotacionDto> Criterios { get; set; } = new();
        // Premio asociado (si existe)
        public PremioDto? Premio { get; set; }
    }

    public class ProyectoResultadoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }= string.Empty;
        public string DescripcionCorta { get; set; } = string.Empty;
        public int VotosCount { get; set; }
        public int SumaValoracion { get; set; }
    }

    public class ComentarioAnonimoDto
    {
        public string Autor { get; set; } = string.Empty;
        public string Etiqueta { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int ProyectoId { get; set; }
        public string ProyectoNombre { get; set; } = string.Empty;
    }
}
