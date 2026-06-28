using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.DTO
{
    public class PremioDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El emoji es obligatorio")]
        [StringLength(8, ErrorMessage = "Emoji demasiado largo")]
        public string Emoji { get; set; }

        [Required(ErrorMessage = "El nombre del premio es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; }

        // Relación con la votación que define este premio
        public int VotacionId { get; set; }

        // Proyecto ganador asignado cuando se cierra la votación (puede ser null hasta asignarse)
        public int? ProyectoGanadorId { get; set; }
    }

    public class PremioCreateDTO
    {
        [Required(ErrorMessage = "El emoji es obligatorio")]
        [StringLength(8, ErrorMessage = "Emoji demasiado largo")]
        public string Emoji { get; set; }

        [Required(ErrorMessage = "El nombre del premio es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; }
    }
}
