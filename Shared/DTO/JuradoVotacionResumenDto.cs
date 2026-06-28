using System;
using Shared.Enums;

namespace Shared.DTO
{
    public class JuradoVotacionResumenDto
    {
        public int Id { get; set; }
        public int EventoId { get; set; }
        public string EventoNombre { get; set; } = string.Empty;
        public string CodigoStr { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int TotalProyectos { get; set; }
        public int ProyectosVotados { get; set; }
    }
}
