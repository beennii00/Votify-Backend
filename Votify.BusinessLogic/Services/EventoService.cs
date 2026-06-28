using Shared.DTO;
using BusinessLogic.Interfaces;
using BusinessLogic.Mappers;
using Domain.Entitites;
using Domain.Factory;
using Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistance.Repositories;

namespace Votify.BusinessLogic.Services
{
    public class EventoService : IEventoService
    {
        private readonly IDAL _dal;

        public EventoService(IDAL dal)
        {
            _dal = dal;
        }

        public async Task<IEnumerable<EventoDTO>> GetAllAsync()
        {
            var eventos = await _dal.Query<Evento>()
                                    .Include(e => e.votacion)
                                    .Include(e => e.proyectos)
                                    .ToListAsync();
            return eventos.Select(e => e.ToDto());
        }

        public async Task<EventoDTO> CreateAsync(EventoDTO dto)
        {
            var fechaInicioUtc = NormalizeToUtc(dto.FechaInicio);
            var fechaFinUtc = NormalizeToUtc(dto.FechaFin);

            if (fechaFinUtc <= fechaInicioUtc)
                throw new ArgumentException("La fecha de fin debe ser posterior a la fecha de inicio.");

            EventoCreator creadorEvento;
            
            if (dto is ESportsEventoDto dtoEsports)
            {
                creadorEvento = new EsportsEventoCreator(dtoEsports.Juego, dtoEsports.Plataforma);
            }
            else
            {
                creadorEvento = new EstandarEventoCreator();
            }

            var evento = creadorEvento.CreateEvento(dto.Nombre, dto.Descripcion ?? "", fechaInicioUtc, fechaFinUtc);
            
            await _dal.InsertAsync(evento);
            await _dal.CommitAsync();
            return evento.ToDto();
        }

        public async Task<EventoDTO> GetByIdAsync(int id)
        {
                        var evento = await _dal.Query<Evento>()
                                               .Include(e => e.votacion)
                                               .Include(e => e.proyectos)
                                               .FirstOrDefaultAsync(e => e.Id == id);
                        if (evento == null) throw new KeyNotFoundException("Evento no encontrado.");
                        return evento.ToDto();
        }

                public async Task<EventoDTO> UpdateAsync(int id, EventoDTO dto)
                {
                        var evento = await _dal.Query<Evento>()
                                               .Include(e => e.votacion)
                                               .Include(e => e.proyectos)
                                               .FirstOrDefaultAsync(e => e.Id == id);
                        if (evento == null) throw new KeyNotFoundException("Evento no encontrado.");

                        var fechaInicioUtc = NormalizeToUtc(dto.FechaInicio);
                        var fechaFinUtc = NormalizeToUtc(dto.FechaFin);

                        if (fechaFinUtc <= fechaInicioUtc)
                                throw new ArgumentException("La fecha de fin debe ser posterior a la fecha de inicio.");

                        // Actualizamos los datos base
                        evento.Nombre = dto.Nombre;
                        evento.Descripcion = dto.Descripcion ?? "";
                        evento.FechaInicio = fechaInicioUtc;
                        evento.FechaFin = fechaFinUtc;

                        // Si es un evento de e-sports, actualizamos sus campos exclusivos
                        if (evento is EsportsEvento esports && dto is ESportsEventoDto dtoEsports)
                        {
                                esports.Juego = dtoEsports.Juego;
                                esports.Plataforma = dtoEsports.Plataforma;
                        }

                        // Al estar rastreado (tracked) por EF Core a través del Query genérico, no hace falta _dal.Update.
                        // Solo con CommitAsync guardaremos los cambios detectados
                        await _dal.CommitAsync();
                        return evento.ToDto();
                }

                public async Task<bool> DeleteAsync(int id)
                {
                        var evento = await _dal.Query<Evento>()
                                               .Include(e => e.votacion)
                                               .Include(e => e.proyectos)
                                               .FirstOrDefaultAsync(e => e.Id == id);
                        if (evento == null) return false;

                        if (evento.votacion != null && evento.votacion.Any(v => v.EstaActiva))
                        {
                                throw new InvalidOperationException("No se puede eliminar. Hay votaciones activas en este evento. Ciérralas primero.");
                        }

                        _dal.Delete(evento);
                        await _dal.CommitAsync();
                        return true;
                }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;

            if (value.Kind == DateTimeKind.Local)
                return value.ToUniversalTime();

            // Cuando llega sin zona (Unspecified), se asume hora local del servidor/API.
            return DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime();
        }
    }
}
