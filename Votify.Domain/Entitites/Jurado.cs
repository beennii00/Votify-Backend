namespace Domain.Entitites
{
    public class Jurado : Usuario
    {
        public ICollection<Votacion> VotacionesParticipadas { get; set; }

        public Jurado() : base()
        {
            VotacionesParticipadas = new List<Votacion>();
        }

        public Jurado(string nombre, string dni, string contrasenya) : base(nombre, dni, contrasenya)
        {
            VotacionesParticipadas = new List<Votacion>();
        }
    }
}
