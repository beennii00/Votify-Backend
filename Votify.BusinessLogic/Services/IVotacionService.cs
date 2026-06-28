using Shared.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IVotacionService
    {
        Task<VotacionDto> CreateAsync(VotacionFormDto dto);
        Task<VotacionDto> UpdateAsync(int eventoId, int votacionId, VotacionFormDto dto);
        Task DeleteAsync(int eventoId, int votacionId);
        Task<DetalleVotacionDto> GetDetalleVotacionAsync(int eventoId, int votacionId);
        Task<(int EventoId, int VotacionId)> ValidarCodigoAccesoAsync(string codigo);
        Task<VotacionDto> AccionVotacionAsync(int eventoId, int votacionId, string accion);
        Task<bool> AsignarJuradosAsync(AsignarJuradosDto dto);
        Task<string> GetResumenIAAsync(int eventoId, int votacionId, int proyectoId);
        Task<bool> YaVotoAsync(int votacionId, int usuarioId);
        
        // Mis Votaciones Jurado
        Task<IEnumerable<JuradoVotacionResumenDto>> GetVotacionesResumenByJuradoAsync(int juradoId);
    }
}
