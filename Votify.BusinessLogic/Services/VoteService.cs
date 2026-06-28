using Shared.DTO;
using Domain.Entitites;
using Persistance.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.Mappers;
using Shared.Enums;

namespace BusinessLogic.Services
{
    public class VoteService : IVoteService
    {
        private readonly IDAL _dal;

        public VoteService(IDAL dal)
        {
            _dal = dal;
        }

        public async Task<bool> ConfigurarFechasVotacionAsync(ConfigurarFechasVotacionDto dto)
        {
            if (dto.FechaFin <= dto.FechaInicio)
            {
                throw new ArgumentException("La fecha de cierre debe ser posterior a la de apertura.");
            }

            var votacion = await _dal.GetByIdAsync<Votacion>(dto.VotacionId);
            if (votacion == null)
            {
                return false; 
            }

            votacion.FechaInicio = dto.FechaInicio;
            votacion.FechaFin = dto.FechaFin;

            await _dal.CommitAsync();

            return true;
        }

        public async Task<VotacionDto?> ObtenerVotacionAsync(int id)
        {
            var votacion = await _dal.GetByIdAsync<Votacion>(id);
            if (votacion == null) return null;
            return votacion.ToDto();
        }

        
        public async Task<bool> EmitirVotoAsync(EmitirVotoDto dto)
        {
            var votacion = await _dal.Query<Votacion>().Include(v => v.Criterios).FirstOrDefaultAsync(v => v.Id == dto.VotacionId);

            if (votacion == null) throw new ArgumentException("Votación no encontrada.");

            ValidarEstadoVotacion(votacion);

            var usuario = await ValidarYObtenerUsuarioAsync(dto, votacion);

            await ValidarAutorizacionJuradoAsync(usuario, dto.VotacionId);

            await ValidarDuplicidadVotoAsync(usuario, dto.VotacionId);
            ValidarLimiteVotos(dto.VotosProyectos.Count, votacion.MaxVotesPerVoter);

            foreach (var vp in dto.VotosProyectos)
            {
                var proyecto = await _dal.GetByIdAsync<Proyecto>(vp.ProyectoId);
                if (proyecto == null) continue;

                // Patrón Estrategia: obtenemos la estrategia y delegamos
                var estrategia = BusinessLogic.Strategies.VoteStrategyFactory.ObtenerEstrategia(votacion.TipoVotacion);
                var voto = estrategia.ProcesarVoto(votacion, proyecto, vp, usuario);

                await _dal.InsertAsync(voto);
            }

            await _dal.CommitAsync();

            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Métodos extraídos (Extract Method – M. Fowler)
        // ─────────────────────────────────────────────────────────────────────
        private static void ValidarEstadoVotacion(Votacion votacion)
        {
            if (votacion.Estado == EstadoVotacion.Pausada)
                throw new ArgumentException("Votación pausada. No se admiten votos en este momento.");

            if (DateTime.UtcNow > votacion.FechaFin || votacion.Estado == EstadoVotacion.Cerrada)
                throw new ArgumentException("La votación ya ha finalizado. No se pueden emitir más votos.");
        }

        private async Task<Usuario?> ValidarYObtenerUsuarioAsync(EmitirVotoDto dto, Votacion votacion)
        {
            if (dto.UsuarioId <= 0)
            {
                if (!votacion.EsVotacionPopular)
                    throw new ArgumentException("Esta votación no es pública. Se requiere un usuario registrado para votar.");
                return null;
            }

            var usuario = await _dal.GetByIdAsync<Usuario>(dto.UsuarioId);
            if (usuario == null) throw new ArgumentException("Usuario no encontrado.");
            return usuario;
        }

        
        private async Task ValidarAutorizacionJuradoAsync(Usuario? usuario, int votacionId)
        {
            if (usuario == null) return;

            var esJurado = await _dal.Query<Votacion>()
                .Where(v => v.Id == votacionId)
                .SelectMany(v => v.JuradosAsignados)
                .AnyAsync(j => j.Id == usuario.Id);

            if (!esJurado)
                throw new ArgumentException("No estás autorizado para votar en esta votación. Solo los jurados asignados pueden participar.");
        }

        private async Task ValidarDuplicidadVotoAsync(Usuario? usuario, int votacionId)
        {
            if (usuario == null) return;

            var yaVoto = await _dal.Query<Voto>()
                .AnyAsync(v => v.Votacion.Id == votacionId && v.NombreUsuario!.Id == usuario.Id);

            if (yaVoto) throw new ArgumentException("Ya has emitido al menos un voto en esta votación.");
        }

        private static void ValidarLimiteVotos(int cantidadVotos, int maxPermitido)
        {
            if (cantidadVotos > maxPermitido)
                throw new ArgumentException($"Máximo permitido: {maxPermitido}.");
        }
    }
}
