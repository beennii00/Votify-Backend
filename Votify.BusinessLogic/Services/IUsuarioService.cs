using Shared.DTO;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDTO> RegistrarAsync(RegisterUsuarioDTO dto);
        Task<UsuarioResponseDTO> IniciarSesionAsync(LoginUsuarioDTO dto);
        Task<UsuarioResponseDTO> ActualizarPerfilAsync(int id, UpdateUsuarioDTO dto);
		Task<List<UsuarioBuscadorDto>> BuscarConcursantesAsync(string query);
		Task<List<UsuarioBuscadorDto>> ObtenerTodosLosJuradosAsync();
		Task<DashboardConcursanteDto> ObtenerDashboardConcursanteAsync(int usuarioId);
		Task CambiarContrasenyaAsync(CambiarContrasenyaDTO dto);
	}
}
