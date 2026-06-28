using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entitites
{
    public abstract class Proyecto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Descripcion { get; set; }
        [Required]
        public Evento Evento { get; set; }

        public string? ResumenIA { get; set; }

        public ICollection<Usuario> Usuarios { get; set; }

        public ICollection<Voto> VotosProyecto { get; set; }


        public Proyecto()
        {
            this.Usuarios = new List<Usuario>();
            this.VotosProyecto = new List<Voto>();
        }

        public void ActualizarDetalles(string nuevoNombre, string nuevaDescripcion)
        {
            if (string.IsNullOrWhiteSpace(nuevoNombre))
                throw new ArgumentException("El nombre del proyecto no puede estar vacío.", nameof(nuevoNombre));

            if (string.IsNullOrWhiteSpace(nuevaDescripcion))
                throw new ArgumentException("La descripción del proyecto no puede estar vacía.", nameof(nuevaDescripcion));

            this.Nombre = nuevoNombre;
            this.Descripcion = nuevaDescripcion;
        }

		public Proyecto(string nombre, string descripcion, Evento evento):this()
        {
            this.Nombre = nombre;
            this.Descripcion = descripcion;

			//relaciones a 1
			this.Evento = evento;
            
            //listas
			this.Usuarios = new List<Usuario>();
			this.VotosProyecto = new List<Voto>();
		}
    }
}

