using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entitites
{
    public abstract class Evento
    {
        [Key]
        public int Id { get; set; }
        [Required]
		public string Nombre { get; set; }
        [Required]
		public string Descripcion { get; set; }
        [Required]
        public DateTime FechaInicio { get; set; }
        [Required]
        public DateTime FechaFin { get; set; }

        //Atributos relaciones
        public ICollection<Votacion> votacion { get; set; }
        public ICollection<Proyecto> proyectos { get; set; }

        protected Evento() 
        {
            votacion = new List<Votacion>();
            proyectos = new List<Proyecto>();
        }
        protected Evento (string nombre, string descripcion, DateTime fechaInicio, DateTime fechaFin):this()
        {
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.FechaInicio = fechaInicio;
            this.FechaFin = fechaFin;
        }

        public void AgregarProyecto(Proyecto proyecto)
        {
            if (proyecto == null) throw new ArgumentNullException(nameof(proyecto));
            
            // Aquí puedes añadir validaciones en el futuro si es necesario
            proyectos.Add(proyecto);
        }

        public void QuitarProyecto(Proyecto proyecto)
        {
            if (proyecto == null) throw new ArgumentNullException(nameof(proyecto));
            
            proyectos.Remove(proyecto);
        }

        public void RemoveVotacion(int votacionId)
        {
            var v = votacion.FirstOrDefault(x => x.Id == votacionId);
            if (v != null)
            {
                votacion.Remove(v);
            }
        }
    }
}
