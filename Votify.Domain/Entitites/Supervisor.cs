namespace Domain.Entitites
{
    public class Supervisor : Usuario
    {
        public Supervisor() : base()
        {
        }

        public Supervisor(string nombre, string dni, string contrasenya) : base(nombre, dni, contrasenya)
        {
        }
    }
}
