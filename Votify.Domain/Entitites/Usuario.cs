using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entitites
{
    public abstract class Usuario
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Nombre { get; set; }
        
        [Required]
        public string Dni { get; set; }
        
        [Required]
        public string Contrasenya { get; set; }

        public string? Avatar { get; set; }

        public ICollection<Voto> voto { get; set; }

        protected Usuario()
        {
            voto = new List<Voto>();
        }

        protected Usuario(string nombre, string dni, string contrasenya) : this()
        {
            this.Nombre = nombre;
            this.Dni = dni;
            this.Contrasenya = contrasenya;
        }

        // Comportamiento de validación encapsulado dentro de la clase
        public bool VerificarContrasenya(string contrasenyaAComprobar)
        {
            return this.Contrasenya == contrasenyaAComprobar;
        }
    }
}

