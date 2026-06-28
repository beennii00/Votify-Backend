using System.ComponentModel.DataAnnotations;

namespace Domain.Entitites
{
    public class CriterioVotacion
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        [Required]
        public decimal Peso { get; set; } // Porcentaje ej: 0.50 para 50%

        [Required]
        public int VotacionId { get; set; }
        public Votacion Votacion { get; set; } = null!;
    }
}