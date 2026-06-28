using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Shared.Enums;

namespace Shared.DTO
{
	public class VotacionFormDto : IValidatableObject
	{
		[Required(ErrorMessage = "El ID del evento es obligatorio")]
		public int EventoId { get; set; }

		[Required(ErrorMessage = "El nombre es obligatorio")]
		[MaxLength(200)]
		public string Nombre { get; set; } = string.Empty;

		[MaxLength(500)]
		public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime? FechaInicio { get; set; }
        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        public DateTime? FechaFin { get; set; }
        public int? MaxVotesPerVoter { get; set; }
        public string? Estado { get; set; }
        public EstadoComentarios EstadoComentarios { get; set; } = EstadoComentarios.Opcionales;
        
        // Propiedades de configuraci�n de acceso
        public bool EsVotacionPopular { get; set; } = false;
        public List<int> JuradosAsignadosIds { get; set; } = new();

		public bool EsPopular { get; set; } = true;

        public TipoVotacion TipoVotacion { get; set; } = TipoVotacion.Numerica;
        public int? ValorMaximoNumerico { get; set; }
        public List<CriterioVotacionDto> Criterios { get; set; } = new();

		// Premio opcional: si se desea asignar uno al primer lugar
		public bool AgregarPremio { get; set; } = false;
		public PremioFormDto? Premio { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (AgregarPremio)
			{
				if (Premio == null)
				{
					yield return new ValidationResult("Debes indicar los datos del premio cuando está activado.", new[] { nameof(Premio) });
				}
				else
				{
					if (string.IsNullOrWhiteSpace(Premio.Nombre))
						yield return new ValidationResult("El nombre del premio es obligatorio.", new[] { "Premio.Nombre" });
					if (string.IsNullOrWhiteSpace(Premio.Emoji))
						yield return new ValidationResult("El emoji del premio es obligatorio.", new[] { "Premio.Emoji" });
				}
			}
		}
	}

	public class PremioFormDto
	{
		// Sin [Required] aquí: la validación condicional se hace en VotacionFormDto.Validate()
		[MaxLength(8)]
		public string Emoji { get; set; } = string.Empty;

		[MaxLength(200)]
		public string Nombre { get; set; } = string.Empty;
	}

    public class CriterioVotacionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del criterio es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int MaxVotesPerVoter { get; set; }
        public bool EstaActiva { get; set; }
        public TimeSpan Duracion { get; set; }
        public bool EsVotacionPopular { get; set; }
        public List<JuradoSeleccionadoDto> JuradosAsignados { get; set; } = new();

        [Required]
        [Range(0.01, 1.0, ErrorMessage = "El peso debe estar entre 0.01 y 1.0")]
        public decimal Peso { get; set; }
    }

	public class VotacionDto
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public string Estado { get; set; } = string.Empty;
		public EstadoComentarios EstadoComentarios { get; set; }
		public string? Descripcion { get; set; }
		public DateTime FechaInicio { get; set; }
		public DateTime FechaFin { get; set; }
		public int MaxVotesPerVoter { get; set; }
		public bool EstaActiva { get; set; }
		public TimeSpan Duracion { get; set; }
		public bool EsPopular { get; set; } 

        public TipoVotacion TipoVotacion { get; set; }
        public int? ValorMaximoNumerico { get; set; }
        public List<CriterioVotacionDto> Criterios { get; set; } = new();
		public PremioDTO? Premio { get; set; }
	}

	public class JuradoSeleccionadoDto
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public string Dni { get; set; } = string.Empty;
	}


	public class AsignarJuradosDto
	{
		[Required(ErrorMessage = "El ID del administrador es obligatorio")]
		public int AdministradorId { get; set; }

		[Required(ErrorMessage = "El ID de la votacion es obligatorio")]
		public int VotacionId { get; set; }

		public List<int> JuradoIds { get; set; } = new();
	}
}