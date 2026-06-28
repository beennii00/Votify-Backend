using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entitites
{
    public class ResumenIAProyecto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VotacionId { get; set; }

        [Required]
        public int ProyectoId { get; set; }

        [Required]
        public string Texto { get; set; }

        [ForeignKey("VotacionId")]
        public virtual Votacion Votacion { get; set; }

        [ForeignKey("ProyectoId")]
        public virtual Proyecto Proyecto { get; set; }
    }
}