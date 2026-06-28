using Domain.Entitites;

namespace Domain.Factory.Usuarios
{
    public class ConcursanteCreator : UsuarioCreator
    {
        public override Usuario CreateUsuario(string nombre, string dni, string contrasenya)
        {
            return new Concursante(nombre, dni, contrasenya);
        }
    }
}
