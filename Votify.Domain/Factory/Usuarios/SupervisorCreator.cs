using Domain.Entitites;

namespace Domain.Factory.Usuarios
{
    public class SupervisorCreator : UsuarioCreator
    {
        public override Usuario CreateUsuario(string nombre, string dni, string contrasenya)
        {
            return new Supervisor(nombre, dni, contrasenya);
        }
    }
}
