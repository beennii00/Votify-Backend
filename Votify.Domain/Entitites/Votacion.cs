using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shared.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.State;
using System.Linq;

namespace Domain.Entitites
{
	public class Votacion
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string Nombre { get; set; } = string.Empty;
		public EstadoVotacion Estado { get; private set; }
		public string? Descripcion { get; set; }

		[NotMapped]
		private IEstadoVotacion? _estadoClase;

		[NotMapped]
		private IEstadoVotacion EstadoActual
		{
			get
			{
				if (_estadoClase == null)
				{
					_estadoClase = Estado switch
					{
						EstadoVotacion.Pendiente => new EstadoPendiente(),
						EstadoVotacion.Activa => new EstadoActiva(),
						EstadoVotacion.Pausada => new EstadoPausada(),
						EstadoVotacion.Cerrada => new EstadoCerrada(),
						_ => new EstadoPendiente()
					};
				}
				return _estadoClase;
			}
		}

		public void SetEstado(IEstadoVotacion s)
		{
			_estadoClase = s;
			Estado = s.CodigoEstado;
		}

		public void Iniciar() => EstadoActual.Iniciar(this);
		public void Pausar() => EstadoActual.Pausar(this);
		public void Reanudar() => EstadoActual.Reanudar(this);
		public void Cerrar() => EstadoActual.Cerrar(this);

		[Required]
		public DateTime FechaInicio { get; set; }
		[Required]
		public DateTime FechaFin { get; set; }
		[Required]
		public int MaxVotesPerVoter { get; set; } = 3;

		public EstadoComentarios EstadoComentarios { get; set; } = EstadoComentarios.Opcionales;
		
		public bool EsVotacionPopular { get; set; }
		public string? CodigoAcceso { get; set; }

		
		
		public List<string> DnisPermitidos { get; set; } = new List<string>();

        // Nuevos campos para plantillas
        public TipoVotacion TipoVotacion { get; set; } = TipoVotacion.Numerica;
        public int? ValorMaximoNumerico { get; set; } // Opcional, usado para Numerica o CriteriosPorPeso
        public ICollection<CriterioVotacion> Criterios { get; set; } = new List<CriterioVotacion>();

		// Atributos relaciones
		public ICollection<Voto> Votos { get; set; }
		public ICollection<Jurado> JuradosAsignados { get; set; }
		[Required]
		public Evento evento { get; set; }  = null!;
		public TimeSpan Duracion => FechaFin - FechaInicio;
		// Premio asociado a la votación (opcional)
		public Premio? Premio { get; set; }
		public bool EstaActiva => DateTime.UtcNow >= FechaInicio && DateTime.UtcNow <= FechaFin && Estado != EstadoVotacion.Pausada && Estado != EstadoVotacion.Cerrada;
		public Votacion()
		{
			this.Votos = new List<Voto>();
			this.JuradosAsignados = new List<Jurado>();
			this.DnisPermitidos = new List<string>();
		}

		public Votacion(string nombre, string? descripcion, DateTime fechaInicio, DateTime fechaFin, int maxVotesPerVoter, Evento evento, EstadoComentarios estadoComentarios, bool esVotacionPopular):this()
		{
			if (fechaFin <= fechaInicio)
				throw new ArgumentException("La fecha de fin debe ser posterior a la de inicio");

			this.Nombre = nombre;
			this.Descripcion = descripcion;
			this.FechaInicio = fechaInicio;
			this.FechaFin = fechaFin;
			this.MaxVotesPerVoter = maxVotesPerVoter;
			this.Estado = EstadoVotacion.Pendiente;
			this.EstadoComentarios = estadoComentarios;
			this.EsVotacionPopular = esVotacionPopular;

			if (esVotacionPopular)
			{
				this.CodigoAcceso = GenerarCodigoAcceso();
			}

			//relaciones a 1
			this.evento = evento;



		}

		private string GenerarCodigoAcceso()
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
		}

		public void Modificar(string? nombre, string? descripcion, DateTime? fechaInicio = null, DateTime? fechaFin = null, EstadoVotacion? estado = null, EstadoComentarios? estadoComentarios = null, int? maxVotesPerVoter = null, bool? esVotacionPopular = null) 
		{
			if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin.Value <= fechaInicio.Value)
				throw new ArgumentException("La fecha de fin debe ser posterior a la de inicio");

			if (!string.IsNullOrWhiteSpace(nombre))
				this.Nombre = nombre;
			if (descripcion != null)
				this.Descripcion = descripcion;
			if (fechaInicio.HasValue)
				this.FechaInicio = fechaInicio.Value;
			if (fechaFin.HasValue)
				this.FechaFin = fechaFin.Value;
			if (estado.HasValue)
				this.Estado = estado.Value;
			if (estadoComentarios.HasValue)
				this.EstadoComentarios = estadoComentarios.Value;
			if (maxVotesPerVoter.HasValue)
				this.MaxVotesPerVoter = maxVotesPerVoter.Value;
			if (esVotacionPopular.HasValue)
				this.EsVotacionPopular = esVotacionPopular.Value;
	}
	}
}