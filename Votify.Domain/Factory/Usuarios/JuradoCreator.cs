using Domain.Entitites;

namespace Domain.Factory.Usuarios
{
    public class JuradoCreator : UsuarioCreator
    {
        public override Usuario CreateUsuario(string nombre, string dni, string contrasenya)
        {
            return new Jurado(nombre, dni, contrasenya);
        }
    }
}
