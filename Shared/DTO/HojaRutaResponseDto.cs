using System;

namespace Shared.DTO
{
    public class HojaRutaResponseDto
    {
        public string Markdown { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    }
}
