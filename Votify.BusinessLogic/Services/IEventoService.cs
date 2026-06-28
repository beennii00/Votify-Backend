using Domain.Entitites;
using Shared.DTO;

namespace BusinessLogic.Interfaces
{
    public interface IEventoService
    {
        Task<IEnumerable<EventoDTO>> GetAllAsync();
        Task<EventoDTO> CreateAsync(EventoDTO dto);
		Task<EventoDTO> GetByIdAsync(int id);
		Task<EventoDTO> UpdateAsync(int id, EventoDTO dto);
		Task<bool> DeleteAsync(int id);
	}
}
