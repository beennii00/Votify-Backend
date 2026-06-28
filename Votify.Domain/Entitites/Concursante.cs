using System.ComponentModel.DataAnnotations;

namespace Domain.Entitites
{
    public class Concursante : Usuario
    {
        public ICollection<Proyecto> ProyectosParticipados { get; set; }

        public Concursante() : base()
        {
            ProyectosParticipados = new List<Proyecto>();
        }

        public Concursante(string nombre, string dni, string contrasenya) : base(nombre, dni, contrasenya)
        {
            ProyectosParticipados = new List<Proyecto>();
        }
    }
}
