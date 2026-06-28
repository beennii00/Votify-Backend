using Domain.Entitites;

namespace Domain.Factory.Usuarios
{
    public class AdministradorCreator : UsuarioCreator
    {
        public override Usuario CreateUsuario(string nombre, string dni, string contrasenya)
        {
            return new Administrador(nombre, dni, contrasenya);
        }
    }
}
