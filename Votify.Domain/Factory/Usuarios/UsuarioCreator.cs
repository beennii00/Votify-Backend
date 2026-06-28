using Domain.Entitites;

namespace Domain.Factory.Usuarios
{
    public abstract class UsuarioCreator
    {
        public abstract Usuario CreateUsuario(string nombre, string dni, string contrasenya);
    }
}
