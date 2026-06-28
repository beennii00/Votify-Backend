using Shared.DTO;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.DTO
{

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "tipoEvento")]
    [JsonDerivedType(typeof(EstandarEventoDto), typeDiscriminator: "Estandar")]
    [JsonDerivedType(typeof(ESportsEventoDto), typeDiscriminator: "Esports")]
    public abstract class EventoDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200)]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        public DateTime FechaFin { get; set; }

        public List<VotacionDto>? Votacion { get; set; }
    }

    public class EstandarEventoDto : EventoDTO
    {
    }


    //Patron Fabrica 
    public class ESportsEventoDto : EventoDTO
    {
        public string Juego { get; set; }
        public string Plataforma { get; set; }
    }

}
