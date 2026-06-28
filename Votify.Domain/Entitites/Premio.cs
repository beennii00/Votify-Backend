using System.ComponentModel.DataAnnotations;

namespace Domain.Entitites
{
    public class Premio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Emoji { get; set; } = string.Empty;

        [Required]
        public string Nombre { get; set; } = string.Empty;

        // Relación 1:1 con Votacion
        public int VotacionId { get; set; }
        public Votacion? Votacion { get; set; }

        // Proyecto ganador (opcional hasta que se asigne al cerrar)
        public int? ProyectoGanadorId { get; set; }
        public Proyecto? ProyectoGanador { get; set; }
    }
}
