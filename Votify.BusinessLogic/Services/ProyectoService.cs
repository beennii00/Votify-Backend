using Shared.DTO;
using Domain.Entitites;
using Domain.Factory;
using Microsoft.EntityFrameworkCore;
using Persistance.Data;
using Persistance.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Mappers;

namespace BusinessLogic.Services
{
    public class ProyectoService : IProyectoService
    {
        private readonly IDAL _dal;

        public ProyectoService(IDAL dal)
        {
            _dal = dal;
        }

        public async Task<List<ProyectoDTO>> ListarProyectosPorEventoAsync(int eventoId)
        {
            var evento = await _dal.Query<Evento>()
                .Include(e => e.proyectos)
                .FirstOrDefaultAsync(e => e.Id == eventoId);

            if (evento == null) 
            {
                return new List<ProyectoDTO>();
            }

            return evento.proyectos.Select(p => p.ToDto()).ToList();
        }

        public async Task<ProyectoDTO> ObtenerProyectoPorIdAsync(int id)
        {
            var p = await _dal.Query<Proyecto>()
                .Include(x => x.Evento)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) 
            {
                return null;
            }

            return p.ToDto();
        }

        public async Task<ProyectoDTO> CrearProyectoAsync(ProyectoDTO proyectoDto)
        {
            var evento = await _dal.GetByIdAsync<Evento>(proyectoDto.EventoId);
            if (evento == null) return null;

            ProyectoCreator factory = null;

            if (proyectoDto is EstandarProyectoDTO)
            {
                factory = new ProyectoEstandarCreator();
            }
            //else if (proyectoDto is OtroProyectoDTO) { ... }

            if (factory == null)
                throw new ArgumentException("Tipo de proyecto no soportado");

            var nProyecto = factory.CreateProyecto(proyectoDto.Nombre, proyectoDto.Descripcion, evento);

            
            evento.AgregarProyecto(nProyecto);
            
            await _dal.CommitAsync();

            proyectoDto.Id = nProyecto.Id;
            return proyectoDto;
        }

        public async Task<ProyectoDTO> ActualizarProyectoAsync(ProyectoDTO proyectoDto)
        {
            var proyecto = await _dal.GetByIdAsync<Proyecto>(proyectoDto.Id);
            if (proyecto == null) return null;

            
            proyecto.ActualizarDetalles(proyectoDto.Nombre, proyectoDto.Descripcion);

            
            await _dal.CommitAsync();
            return proyectoDto;
        }

        public async Task<bool> EliminarProyectoAsync(int id)
        {
            var proyecto = await _dal.Query<Proyecto>()
                .Include(p => p.Evento)
                    .ThenInclude(e => e.proyectos) 
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proyecto == null) return false;

            proyecto.Evento.QuitarProyecto(proyecto);

            
            _dal.Delete(proyecto); 


            await _dal.CommitAsync();
            return true;
        }

		public async Task AsignarEquipoAsync(AsignarConcursanteDto dto)
		{
			var proyecto = await _dal.Query<Proyecto>()
				.Include(p => p.Usuarios)
				.FirstOrDefaultAsync(p => p.Id == dto.ProyectoId);

			if (proyecto == null)
			{
				throw new Exception($"El proyecto con ID {dto.ProyectoId} no existe.");
			}

			var nuevosConcursantes = await _dal.Query<Usuario>()
				.Where(u => dto.ConcursantesIds.Contains(u.Id))
				.ToListAsync();

			proyecto.Usuarios.Clear();

			foreach (var concursante in nuevosConcursantes)
			{
				proyecto.Usuarios.Add(concursante);
			}

			await _dal.CommitAsync();
		}

		public async Task<List<UsuarioBuscadorDto>> ObtenerEquipoProyectoAsync(int proyectoId)
		{
			var proyecto = await _dal.Query<Proyecto>()
				.Include(p => p.Usuarios)
				.FirstOrDefaultAsync(p => p.Id == proyectoId);

			if (proyecto == null)
			{
				return new List<UsuarioBuscadorDto>();
			}

			return proyecto.Usuarios.Select(u => new UsuarioBuscadorDto
			{
				Id = u.Id,
				Nombre = u.Nombre,
				Dni = u.Dni
			}).ToList();
		}
	}
}
