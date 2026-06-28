using Domain.Entitites;
using Microsoft.EntityFrameworkCore;
using Persistance.Data;
using Persistence.Interfaces;

namespace Persistence.Repositories
{
    public class EventoRepository : IEventoRepository
    {
        private readonly AppDbContext _context;

        public EventoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Evento>> GetAllAsync()
            => await _context.Set<Evento>()
                .Include(e => e.votacion)
                .Include(e => e.proyectos)
                .ToListAsync();

        public async Task<Evento> CreateAsync(Evento evento)
        {
            _context.Set<Evento>().Add(evento);
            await _context.SaveChangesAsync();
            return evento;
        }

        public async Task<Evento?> GetByIdAsync(int id)
        {
            return await _context.Set<Evento>()
                .Include(e => e.votacion)
                .Include(e => e.proyectos)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task UpdateAsync(Evento evento)
        {
            _context.Set<Evento>().Update(evento);
            await _context.SaveChangesAsync();
        }
		public async Task DeleteAsync(Evento evento)
		{
			_context.Set<Evento>().Remove(evento);
			await _context.SaveChangesAsync();
		}
	}
}