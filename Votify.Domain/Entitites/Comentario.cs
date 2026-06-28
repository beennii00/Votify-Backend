using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitites
{
    public class Comentario
    {
        //Atributos objeto
            [Key]
            public int Id { get; set; }
            [Required]
            public string Contenido { get; set; }
            [Required]
            public DateTime FechaCreacion { get; set; }

            //Atributos relaciones
            [Required]
            public int VotoId { get; set; }
            [ForeignKey(nameof(VotoId))]
            public Voto Voto { get; set; }
            // Si el comentario está asociado a un criterio específico, se guarda aquí
            public int? CriterioId { get; set; }

        public Comentario()
		{
			
		}
        public Comentario(string contenido, DateTime fechaCreacion, Voto voto, int? criterioId = null):this()
        {
            this.Contenido = contenido;
            this.FechaCreacion = fechaCreacion;
            this.Voto = voto;
            this.CriterioId = criterioId;
        }


		/*public int UsuarioId { get; set; }
        public int PostagemId { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual Postagem Postagem { get; set; }*/
	}
}
