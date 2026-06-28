using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entitites
{
    public class Voto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime FechaVotado { get; set; }
        
        public int? Valoracion { get; set; } // Opcional, usado para Binaria (0/1) y Numérica

        public string? CriterioValoracionesJson { get; set; } // Usado para ponderada por criterios, guarda un JSON

        //Atributos relaciones
        public Usuario? NombreUsuario { get; set; }
        public List<Comentario> Comentarios { get; set; } = new();
        [Required]
        public Votacion Votacion { get; set; } = null!;
        [Required]
        public Proyecto Proy { get; set; } = null!;

        public Voto()
        {
        }

        public Voto(DateTime fechaVotado, Usuario? usuario, Votacion votacion, Proyecto proy, int valoracion = 0):this()
        {
            this.FechaVotado = fechaVotado;
            this.NombreUsuario = usuario;
            this.Votacion = votacion;
            this.Proy = proy;
            this.Valoracion = valoracion;
        }
    }
}
