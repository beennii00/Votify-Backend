using Shared.DTO;
using BusinessLogic.Interfaces;
using BusinessLogic.Mappers;
using Domain.Entitites;
using Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Votify.BusinessLogic.Services
{
    public class VotacionService : IVotacionService
    {
        private readonly IDAL _dal;
        private readonly IIACommentSynthesisService _iaService;

        public VotacionService(IDAL dal, IIACommentSynthesisService iaService)
        {
            _dal = dal;
            _iaService = iaService;
        }

        public async Task<string> GetResumenIAAsync(int eventoId, int votacionId, int proyectoId)
        {
            var votacion = await _dal.Query<Votacion>().FirstOrDefaultAsync(v => v.Id == votacionId && v.evento.Id == eventoId);
            if (votacion == null) throw new KeyNotFoundException("Votación no encontrada.");
            
            bool isFinalizada = (DateTime.UtcNow > votacion.FechaFin) || (votacion.Estado == Domain.Entitites.EstadoVotacion.Cerrada);
            if (!isFinalizada) 
                return "El resumen IA solo está disponible para votaciones finalizadas.";


            var proyecto = await _dal.Query<Proyecto>().FirstOrDefaultAsync(p => p.Id == proyectoId && p.Evento.Id == eventoId);
            if (proyecto == null) throw new KeyNotFoundException("Proyecto no encontrado.");

            var comentarios = await _dal.Query<Comentario>()
                .Where(c => c.Voto.Votacion.Id == votacionId && c.Voto.Proy.Id == proyectoId)
                .Select(c => c.Contenido)
                .ToListAsync();

            if (comentarios.Count < 2)
                return "No hay suficientes comentarios para generar un resumen.";

            return await _iaService.ResumirComentariosAsync(proyectoId, comentarios);
        }

        public async Task<VotacionDto> CreateAsync(VotacionFormDto dto)
        {
            var evento = await _dal.Query<Evento>()
                                   .Include(e => e.votacion)
                                   .Include(e => e.proyectos)
                                   .FirstOrDefaultAsync(e => e.Id == dto.EventoId);
            if (evento == null)
            {
                throw new KeyNotFoundException("Evento no encontrado.");
            }

            var fechaInicio = dto.FechaInicio!.Value.ToUniversalTime();
            var fechaFin = dto.FechaFin!.Value.ToUniversalTime();
            var maxVotes = dto.MaxVotesPerVoter ?? 3;

            var votacion = new Votacion(
                dto.Nombre,
                dto.Descripcion,
                fechaInicio,
                fechaFin,
                maxVotes,
                evento,
                dto.EstadoComentarios,
				dto.EsVotacionPopular
			);
            
            // Asignación de indicador (Asegúrate de agregar este campo en la Entidad Votacion.cs)
            // votacion.EsVotacionPopular = dto.EsVotacionPopular;

            // Procesar y asignar los Jurados enviados en el formulario.
            if (dto.JuradosAsignadosIds != null && dto.JuradosAsignadosIds.Any())
            {
                var jurados = await _dal.Query<Usuario>().OfType<Jurado>()
                                        .Where(j => dto.JuradosAsignadosIds.Contains(j.Id))
                                        .ToListAsync();
                foreach (var jurado in jurados)
                {
					votacion.JuradosAsignados.Add(jurado);
                }
            }

			// template fields
			votacion.TipoVotacion = dto.TipoVotacion;
			votacion.ValorMaximoNumerico = dto.ValorMaximoNumerico;
			if (dto.Criterios != null && dto.Criterios.Count > 0)
			{
				votacion.Criterios = dto.Criterios.Select(c => new CriterioVotacion
				{
					Nombre = c.Nombre,
					Descripcion = c.Descripcion,
					Peso = c.Peso
				}).ToList();
			}

			if (dto.EsPopular)
			{
				votacion.DnisPermitidos.Clear();
			}

            if (dto.AgregarPremio)
            {
                if (dto.Premio == null || string.IsNullOrWhiteSpace(dto.Premio.Nombre))
                    throw new ArgumentException("Si activas el premio, debes indicar el nombre del premio.");

                var emoji = string.IsNullOrWhiteSpace(dto.Premio.Emoji) ? "🏆" : dto.Premio.Emoji;
                var premio = new Premio
                {
                    Emoji = emoji,
                    Nombre = dto.Premio.Nombre,
                    Votacion = votacion
                };
                await _dal.InsertAsync(premio);
            }

            await _dal.InsertAsync(votacion);

            await _dal.CommitAsync();

			return votacion.ToDto()!;
		}

		public async Task<VotacionDto> UpdateAsync(int eventoId, int votacionId, VotacionFormDto dto)
		{
			var evento = await _dal.Query<Evento>()
								   .Include(e => e.votacion)
								   .Include(e => e.proyectos)
								   .FirstOrDefaultAsync(e => e.Id == eventoId);
			if (evento == null)
				throw new KeyNotFoundException("Evento no encontrado.");

            var votacion = await _dal.Query<Votacion>()
                                     .Include(v => v.JuradosAsignados)
                                     .Include(v => v.Criterios)
                                     .FirstOrDefaultAsync(v => v.Id == votacionId);

            if (votacion == null)
            {
                throw new KeyNotFoundException("Votación no encontrada en este evento.");
            }

            EstadoVotacion? nuevoEstado = null;
            if (dto.Estado != null && Enum.TryParse<EstadoVotacion>(dto.Estado, true, out var parsedEstado)) 
            {
                nuevoEstado = parsedEstado;
            }

			votacion.Modificar(
				dto.Nombre,
				dto.Descripcion,
				dto.FechaInicio?.ToUniversalTime(),
				dto.FechaFin?.ToUniversalTime(),
				nuevoEstado,
				dto.EstadoComentarios,
                dto.MaxVotesPerVoter,
                dto.EsVotacionPopular
			);

			if (dto.JuradosAsignadosIds != null)
            {
                votacion.JuradosAsignados.Clear();
                if (dto.JuradosAsignadosIds.Any())
                {
                    var jurados = await _dal.Query<Usuario>().OfType<Jurado>()
                                            .Where(j => dto.JuradosAsignadosIds.Contains(j.Id))
                                            .ToListAsync();
                    foreach (var jurado in jurados)
                    {
						votacion.JuradosAsignados.Add(jurado);
                    }
                }
            }

			// update template fields
			votacion.TipoVotacion = dto.TipoVotacion;
			votacion.ValorMaximoNumerico = dto.ValorMaximoNumerico;
            votacion.Criterios ??= new List<CriterioVotacion>();
            var criteriosActuales = votacion.Criterios.Where(c => c.Id > 0).ToDictionary(c => c.Id);
            var criteriosSincronizados = new List<CriterioVotacion>();

            if (dto.Criterios != null && dto.Criterios.Count > 0)
            {
                foreach (var criterioDto in dto.Criterios)
                {
                    if (criterioDto.Id > 0 && criteriosActuales.TryGetValue(criterioDto.Id, out var criterioExistente))
                    {
                        criterioExistente.Nombre = criterioDto.Nombre;
                        criterioExistente.Descripcion = criterioDto.Descripcion;
                        criterioExistente.Peso = criterioDto.Peso;
                        criteriosSincronizados.Add(criterioExistente);
                    }
                    else
                    {
                        criteriosSincronizados.Add(new CriterioVotacion
                        {
                            Nombre = criterioDto.Nombre,
                            Descripcion = criterioDto.Descripcion,
                            Peso = criterioDto.Peso
                        });
                    }
                }
            }

            votacion.Criterios.Clear();
            foreach (var criterio in criteriosSincronizados)
            {
                votacion.Criterios.Add(criterio);
            }

			if (dto.EsPopular)
			{
				votacion.DnisPermitidos.Clear();
			}

            if (dto.AgregarPremio)
            {
                if (dto.Premio == null || string.IsNullOrWhiteSpace(dto.Premio.Nombre))
                    throw new ArgumentException("Si activas el premio, debes indicar el nombre del premio.");

                if (votacion.Premio == null)
                {
                    votacion.Premio = new Premio();
                }

                votacion.Premio.Emoji = string.IsNullOrWhiteSpace(dto.Premio.Emoji) ? "🏆" : dto.Premio.Emoji;
                votacion.Premio.Nombre = dto.Premio.Nombre;
            }
            else if (votacion.Premio != null)
            {
                votacion.Premio = null;
            }

			await _dal.CommitAsync();
			return votacion.ToDto()!;
		}

		public async Task DeleteAsync(int eventoId, int votacionId)
		{
			var evento = await _dal.Query<Evento>()
								   .Include(e => e.votacion)
								   .Include(e => e.proyectos)
								   .FirstOrDefaultAsync(e => e.Id == eventoId);
			if (evento == null)
				throw new KeyNotFoundException("Evento no encontrado.");

			evento.RemoveVotacion(votacionId);
			await _dal.CommitAsync();
		}

		public async Task<DetalleVotacionDto> GetDetalleVotacionAsync(int eventoId, int votacionId)
        {
            var votacion = await _dal.Query<Votacion>()
                .Include(v => v.evento)
                .Include(v => v.JuradosAsignados)
                .Include(v => v.Criterios)
                .Include(v => v.Premio)
                    .ThenInclude(p => p.ProyectoGanador)
                .FirstOrDefaultAsync(v => v.Id == votacionId && v.evento.Id == eventoId);

            if (votacion == null)
                throw new KeyNotFoundException("Votación no encontrada.");

            var resultados = await ObtenerResultadosAgrupadosAsync(votacionId);
 
            // 3. Traer los comentarios de forma independiente
            var comentarios = await ObtenerComentariosAnonimosAsync(votacionId);

            // 4. Calcular métricas de participación
            var totalJurados = votacion.JuradosAsignados.Count;
                
            // Votantes únicos reales (intención de voto real)
            var votantesCuentasUnicas = await _dal.Query<Voto>()
                .Where(v => v.Votacion.Id == votacionId && v.NombreUsuario != null)
                .Select(v => v.NombreUsuario!.Id)
                .Distinct()
                .CountAsync();
			var juradosDto = votacion.JuradosAsignados.Select(j => new UsuarioBuscadorDto { Id = j.Id, Nombre = j.Nombre, Dni = j.Dni }).ToList();

            // 5. Construir y devolver el DTO final
            return ConstruirDtoDetalle(votacion, resultados, comentarios, totalJurados, votantesCuentasUnicas, juradosDto);
        }

        public async Task<(int EventoId, int VotacionId)> ValidarCodigoAccesoAsync(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("El código no puede estar vacío");

            var votacion = await _dal.Query<Votacion>()
                .Include(v => v.evento)
                .FirstOrDefaultAsync(v => v.CodigoAcceso == codigo);

            if (votacion == null || !votacion.EsVotacionPopular)
            {
                throw new KeyNotFoundException("Código de acceso inválido o no pertecene a una votación pública.");
            }

            var now = DateTime.UtcNow;
            if (now < votacion.FechaInicio || now > votacion.FechaFin)
            {
                throw new InvalidOperationException("La votación no está activa en este momento.");
            }

            return (votacion.evento.Id, votacion.Id);
        }

        private async Task<List<ProyectoResultadoDto>> ObtenerResultadosAgrupadosAsync(int votacionId)
        {
            return await _dal.Query<Voto>()
                .Where(v => v.Votacion.Id == votacionId && v.Proy != null)
                .GroupBy(v => new { v.Proy.Id, v.Proy.Nombre, v.Proy.Descripcion })
                .Select(g => new ProyectoResultadoDto
                {
                    Id = g.Key.Id,
                    Nombre = g.Key.Nombre,
                    DescripcionCorta = g.Key.Descripcion ?? "Sin descripción",
                    VotosCount = g.Count(),
                    SumaValoracion = g.Sum(x => x.Valoracion ?? 0)
                })
                .OrderByDescending(r => r.SumaValoracion)
                .ThenByDescending(r => r.VotosCount)
                .ToListAsync();
        }

		private async Task<List<ComentarioAnonimoDto>> ObtenerComentariosAnonimosAsync(int votacionId)
		{
            // Cargar nombres de criterios para la votación
            var criteriosDict = await _dal.Query<CriterioVotacion>()
                .Where(cr => cr.Votacion.Id == votacionId)
                .ToDictionaryAsync(cr => cr.Id, cr => cr.Nombre);

            var rawComments = await _dal.Query<Comentario>()
                .Where(c => c.Voto != null && c.Voto.Votacion.Id == votacionId && c.Contenido != null && c.Contenido != "")
                .OrderByDescending(c => c.FechaCreacion)
                .Select(c => new
                {
                    ProyectoId = c.Voto != null && c.Voto.Proy != null ? c.Voto.Proy.Id : 0,
                    ProyectoNombre = c.Voto != null && c.Voto.Proy != null ? c.Voto.Proy.Nombre : "Sin proyecto",
                    CriterioId = c.CriterioId,
                    Contenido = c.Contenido,
                    Fecha = c.FechaCreacion
                })
                .ToListAsync();

            return rawComments.Select((c, index) => new ComentarioAnonimoDto
            {
                Autor = $"Anónimo #{rawComments.Count - index}",
                Etiqueta = c.CriterioId.HasValue && criteriosDict.TryGetValue(c.CriterioId.Value, out var critName)
                    ? $"{c.ProyectoNombre} — {critName}"
                    : c.ProyectoNombre,
                Contenido = c.Contenido,
                Fecha = c.Fecha,
                ProyectoId = c.ProyectoId,
                ProyectoNombre = c.ProyectoNombre
            }).ToList();
		}

        private DetalleVotacionDto ConstruirDtoDetalle(
            Votacion votacion, 
            List<ProyectoResultadoDto> resultados, 
            List<ComentarioAnonimoDto> comentarios, 
            int totalJurados, 
            int votantesUnicos,
            List<UsuarioBuscadorDto> juradosAsignados)
        {
            var porcentajeParticipacion = totalJurados > 0 ? (int)Math.Round((double)votantesUnicos / totalJurados * 100) : 0;
            
            var lider = resultados.FirstOrDefault();
            var estadoDinamico = "Pendiente de iniciar";
            
            // Reemplazo del "Límite Máximo Posible" al cálculo por "Cuota de Voto (Share of Voice)"
            var totalPuntosRepartidos = resultados.Sum(r => r.SumaValoracion);
            var liderPorcentaje = (totalPuntosRepartidos > 0 && lider != null)
                ? (int)Math.Round((double)lider.SumaValoracion / totalPuntosRepartidos * 100) 
                : 0;
            if (votacion.Estado == EstadoVotacion.Cerrada || DateTime.UtcNow > votacion.FechaFin) estadoDinamico = "Cerrada";
            else if (votacion.Estado == EstadoVotacion.Pausada) estadoDinamico = "Pausada";
            else if (votacion.Estado == EstadoVotacion.Activa || DateTime.UtcNow >= votacion.FechaInicio) estadoDinamico = "Activa";

			return new DetalleVotacionDto
			{
				Id = votacion.Id,
				Nombre = votacion.Nombre,
				Descripcion = votacion.Descripcion ?? "",
				Categoria = votacion.evento switch
				{
					EsportsEvento => "Esports",
					EstandarEvento => "Estandar",
					_ => "General"
				},
				EstadoComentarios = votacion.EstadoComentarios,
				Estado = estadoDinamico,
                MaxVotesPerVoter = votacion.MaxVotesPerVoter,
                EstaActiva = votacion.EstaActiva,
                EsVotacionPopular = votacion.EsVotacionPopular,
                CodigoAcceso = votacion.CodigoAcceso,
                FechaInicio = votacion.FechaInicio,
                FechaFin = votacion.FechaFin,
                VotantesUnicos = votantesUnicos,
                TotalJurados = totalJurados,
                ParticipacionPorcentaje = porcentajeParticipacion,
                ProyectosEnCarrera = resultados.Count(r => r.VotosCount > 0),
                LiderProvisional = lider?.Nombre ?? "Sin líder",
                LiderVotos = lider?.SumaValoracion ?? 0,
                LiderPorcentaje = liderPorcentaje,
                ResultadosProvisionales = resultados,
                Comentarios = comentarios,
                Jurados = juradosAsignados,
				// Template info
				TipoVotacion = votacion.TipoVotacion,
				ValorMaximoNumerico = votacion.ValorMaximoNumerico,
				Criterios = votacion.Criterios?.Select(c => new CriterioVotacionDto
				{
					Id = c.Id,
					Nombre = c.Nombre,
					Descripcion = c.Descripcion,
					Peso = c.Peso
				}).ToList() ?? new List<CriterioVotacionDto>()
                    ,
                    Premio = votacion.Premio == null ? null : new Shared.DTO.PremioDto
                    {
                        Id = votacion.Premio.Id,
                        Emoji = votacion.Premio.Emoji,
                        Nombre = votacion.Premio.Nombre,
                        ProyectoGanadorId = votacion.Premio.ProyectoGanadorId,
                        ProyectoGanadorNombre = votacion.Premio.ProyectoGanador != null ? votacion.Premio.ProyectoGanador.Nombre : null,
                        ProyectoGanadorConcursantes = votacion.Premio.ProyectoGanador != null
                            ? votacion.Premio.ProyectoGanador.Usuarios.Select(u => new UsuarioBuscadorDto { Id = u.Id, Nombre = u.Nombre, Dni = u.Dni }).ToList()
                            : new List<UsuarioBuscadorDto>()
                    }
			};
		}

		public async Task<VotacionDto> AccionVotacionAsync(int eventoId, int votacionId, string accion)
		{
			var evento = await _dal.Query<Evento>()
								   .Include(e => e.votacion)
								   .Include(e => e.proyectos)
								   .FirstOrDefaultAsync(e => e.Id == eventoId);
			if (evento == null) throw new KeyNotFoundException("Evento no encontrado.");
			var votacion = evento.votacion.FirstOrDefault(v => v.Id == votacionId);
			if (votacion == null) throw new KeyNotFoundException("Votación no encontrada.");

            await AplicarAccionEstadoAsync(votacion, accion);

            await _dal.CommitAsync();
            return votacion.ToDto()!;
        }

        private async Task AplicarAccionEstadoAsync(Votacion votacion, string accion)
        {
            switch (accion.ToLower())
            {
                case "iniciar":
                    votacion.Iniciar();
                    break;
                case "finalizar":
                case "cerrar":
                    votacion.Cerrar();
                    // Si la votación tiene un premio configurado y aún no tiene proyecto ganador,
                    // asignar automáticamente el primer proyecto clasificado.
                    if (votacion.Premio != null && !votacion.Premio.ProyectoGanadorId.HasValue)
                    {
                        var resultados = await ObtenerResultadosAgrupadosAsync(votacion.Id);
                        var lider = resultados.FirstOrDefault();
                        if (lider != null)
                        {
                            // Cargar el proyecto ganador con sus concursantes y asignarlo al premio
                            var proyectoGanador = await _dal.Query<Proyecto>()
                                .Include(p => p.Usuarios)
                                .FirstOrDefaultAsync(p => p.Id == lider.Id);

                            votacion.Premio.ProyectoGanadorId = lider.Id;
                            if (proyectoGanador != null)
                            {
                                votacion.Premio.ProyectoGanador = proyectoGanador;
                            }
                        }
                    }
                    break;
                case "detener":
                    votacion.Pausar();
                    break;
                case "reanudar":
                    votacion.Reanudar();
                    break;
                case "extender":
                    votacion.FechaFin = votacion.FechaFin.AddHours(2);
                    break;
                default:
                    throw new ArgumentException("Acción no válida.");
            }
        }

        public async Task<bool> AsignarJuradosAsync(AsignarJuradosDto dto)
        {
            var esAdmin = await _dal.Query<Usuario>().OfType<Administrador>().AnyAsync(a => a.Id == dto.AdministradorId);
            if (!esAdmin)
            {
                throw new UnauthorizedAccessException("Solo un administrador puede seleccionar jurados.");
            }

            var votacion = await _dal.Query<Votacion>()
                .Include(v => v.JuradosAsignados)
                .FirstOrDefaultAsync(v => v.Id == dto.VotacionId);

            if (votacion == null)
            {
                throw new KeyNotFoundException("Votacion no encontrada.");
            }

            var idsSolicitados = (dto.JuradoIds ?? new List<int>()).Distinct().ToList();
            var juradosSeleccionados = await _dal.Query<Usuario>().OfType<Jurado>()
                .Where(j => idsSolicitados.Contains(j.Id))
                .ToListAsync();

            votacion.JuradosAsignados.Clear();
            foreach (var jurado in juradosSeleccionados)
            {
                votacion.JuradosAsignados.Add(jurado);
            }

            await _dal.CommitAsync();

            return true;
        }

        public async Task<bool> YaVotoAsync(int votacionId, int usuarioId)
        {
            return await _dal.Query<Voto>()
                .AnyAsync(v => v.Votacion.Id == votacionId && v.NombreUsuario!.Id == usuarioId);
        }

        public async Task<IEnumerable<JuradoVotacionResumenDto>> GetVotacionesResumenByJuradoAsync(int juradoId)
        {
            var votaciones = await _dal.Query<Votacion>()
                .Include(v => v.evento)
                .Include(v => v.JuradosAsignados)
                .Where(v => v.JuradosAsignados.Any(j => j.Id == juradoId))
                .ToListAsync();

            var result = new List<JuradoVotacionResumenDto>();

            foreach(var v in votaciones)
            {
                var eventoId = v.evento?.Id ?? 0;
                var eventoNombre = v.evento?.Nombre ?? "Evento Desconocido";
                var totalProyectos = await _dal.Query<Proyecto>().CountAsync(p => p.Evento.Id == eventoId);
                var proyectosVotados = await _dal.Query<Voto>().CountAsync(voto => voto.Votacion.Id == v.Id && voto.NombreUsuario!.Id == juradoId);

                result.Add(new JuradoVotacionResumenDto
                {
                    Id = v.Id,
                    EventoId = eventoId,
                    EventoNombre = eventoNombre,
                    CodigoStr = $"VOT{v.Id:D5}",
                    Titulo = v.Nombre,
                    Descripcion = v.Descripcion,
                    FechaInicio = v.FechaInicio,
                    FechaFin = v.FechaFin,
                    Estado = v.Estado.ToString(),
                    TotalProyectos = totalProyectos,
                    ProyectosVotados = proyectosVotados
                });
            }

            return result;
        }
    }
}