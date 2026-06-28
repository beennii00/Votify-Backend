using Domain.Entitites;

namespace Persistence.Interfaces
{
    public interface IEventoRepository
    {
        Task<IEnumerable<Evento>> GetAllAsync();
        Task<Evento?> GetByIdAsync(int id);
        Task<Evento> CreateAsync(Evento evento);
        Task UpdateAsync(Evento evento);
		Task DeleteAsync(Evento evento);
	}
}