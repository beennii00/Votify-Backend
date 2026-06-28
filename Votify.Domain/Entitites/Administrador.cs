namespace Domain.Entitites
{
    public class Administrador : Usuario
    {
        public Administrador() : base()
        {
        }

        public Administrador(string nombre, string dni, string contrasenya) : base(nombre, dni, contrasenya)
        {
        }
    }
}
