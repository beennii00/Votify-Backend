using Shared.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public interface IProyectoService
    {
        Task<List<ProyectoDTO>> ListarProyectosPorEventoAsync(int eventoId);
        Task<ProyectoDTO> ObtenerProyectoPorIdAsync(int id);
        Task<ProyectoDTO> CrearProyectoAsync(ProyectoDTO proyectoDto);
        Task<ProyectoDTO> ActualizarProyectoAsync(ProyectoDTO proyectoDto);
        Task<bool> EliminarProyectoAsync(int id);
		Task AsignarEquipoAsync(AsignarConcursanteDto dto);
		Task<List<UsuarioBuscadorDto>> ObtenerEquipoProyectoAsync(int proyectoId);
	}
}
