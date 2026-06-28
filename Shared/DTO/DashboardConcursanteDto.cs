using System;
using System.Collections.Generic;

namespace Shared.DTO
{
    public class DashboardConcursanteDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public int TotalProyectos { get; set; }
        public int TotalPremios { get; set; }
        public List<ProyectoDashboardDto> Proyectos { get; set; } = new List<ProyectoDashboardDto>();
        public List<PremioDashboardDto> Premios { get; set; } = new List<PremioDashboardDto>();
    }

    public class ProyectoDashboardDto
    {
        public int Id { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;
        public int EventoId { get; set; }
        public string NombreEvento { get; set; } = string.Empty;
        public string EstadoEvento { get; set; } = string.Empty;
        public DateTime? FechaInicioEvento { get; set; }
        public DateTime? FechaFinEvento { get; set; }
        public int TotalVotaciones { get; set; }
        public int TotalComentarios { get; set; }
        
        public List<VotacionDashboardDto> Votaciones { get; set; } = new List<VotacionDashboardDto>();
        public List<ComentarioDashboardDto> Comentarios { get; set; } = new List<ComentarioDashboardDto>();
    }

    public class VotacionDashboardDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int? Posicion { get; set; }
        public decimal Puntuacion { get; set; }
        public int TotalVotos { get; set; }
    }

    public class ComentarioDashboardDto
    {
        public string Autor { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
    }

    public class PremioDashboardDto
    {
        public int Id { get; set; }
        public string Emoji { get; set; } = string.Empty;
        public string NombrePremio { get; set; } = string.Empty;
        public string NombreEvento { get; set; } = string.Empty;
        public string NombreVotacion { get; set; } = string.Empty;
        public int? Posicion { get; set; }
        public decimal? Puntuacion { get; set; }
        public string TipoGanador { get; set; } = string.Empty;
    }
}
