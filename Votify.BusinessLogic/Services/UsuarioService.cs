using Shared.DTO;
using Domain.Entitites;
using Domain.Factory.Usuarios;
using Microsoft.EntityFrameworkCore;
using Persistance.Data;
using Persistance.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Votify.BusinessLogic.Services; // Add this line if needed

namespace BusinessLogic.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IDAL _dal;
        private readonly IIACommentSynthesisService _iaService;

        public UsuarioService(IDAL dal, IIACommentSynthesisService iaService)
        {
            _dal = dal;
            _iaService = iaService;
        }

        public async Task<UsuarioResponseDTO> RegistrarAsync(RegisterUsuarioDTO dto)
        {
            var existe = await _dal.Query<Usuario>().AnyAsync(u => u.Dni == dto.Dni);
            if (existe)
            {
                throw new Exception("La información introducida indica que el usuario ya se encuentra registrado o contiene errores.");
            }

            UsuarioCreator creator = dto.Rol.ToLower() switch
            {
                "concursante" => new ConcursanteCreator(),
                "jurado" => new JuradoCreator(),
                "administrador" => new AdministradorCreator(),
                "supervisor" => new SupervisorCreator(),
                _ => throw new Exception("El rol especificado no es v�lido.")
            };

            Usuario nuevoUsuario = creator.CreateUsuario(dto.Nombre, dto.Dni, dto.Contrasenya);

            await _dal.InsertAsync(nuevoUsuario);
            await _dal.CommitAsync();

            return new UsuarioResponseDTO
            {
                Id = nuevoUsuario.Id,
                Nombre = nuevoUsuario.Nombre,
                Dni = nuevoUsuario.Dni,
                Rol = nuevoUsuario switch
                {
                    Administrador => "Administrador",
                    Jurado => "Jurado",
                    Supervisor => "Supervisor",
                    Concursante => "Concursante",
                    _ => "Usuario"
                }
            };
        }

        public async Task<UsuarioResponseDTO> IniciarSesionAsync(LoginUsuarioDTO dto)
        {
            var usuario = await _dal.Query<Usuario>().FirstOrDefaultAsync(u => u.Dni == dto.Dni);

            if (usuario == null || !usuario.VerificarContrasenya(dto.Contrasenya))
            {
                throw new Exception("DNI o Contrasenya incorrectos.");
            }

            return new UsuarioResponseDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Dni = usuario.Dni,
                Avatar = usuario.Avatar,
                Rol = usuario switch
                {
                    Administrador => "Administrador",
                    Jurado => "Jurado",
                    Supervisor => "Supervisor",
                    Concursante => "Concursante",
                    _ => "Usuario"
                }
            };
        }

        public async Task<UsuarioResponseDTO> ActualizarPerfilAsync(int id, UpdateUsuarioDTO dto)
        {
            var usuario = await _dal.Query<Usuario>().FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado.");
            }

            usuario.Nombre = dto.Nombre;
            
            // Lógica de contraseña
            if (!string.IsNullOrWhiteSpace(dto.Contrasenya))
            {
                if (dto.Contrasenya != dto.ConfirmarContrasenya)
                {
                    throw new Exception("Las contraseñas no coinciden.");
                }
                usuario.Contrasenya = dto.Contrasenya;
            }

            if (dto.Avatar != null)
            {
                usuario.Avatar = dto.Avatar;
            }

            await _dal.CommitAsync();

            return new UsuarioResponseDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Dni = usuario.Dni,
                Avatar = usuario.Avatar,
                Rol = usuario switch
                {
                    Administrador => "Administrador",
                    Jurado => "Jurado",
                    Supervisor => "Supervisor",
                    Concursante => "Concursante",
                    _ => "Usuario"
                }
            };
        }

		public async Task<List<UsuarioBuscadorDto>> BuscarConcursantesAsync(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
				return new List<UsuarioBuscadorDto>();

			query = query.ToLower();

			var resultados = await _dal.Query<Usuario>()
				.OfType<Concursante>()
				.Where(u => u.Nombre.ToLower().Contains(query) || u.Dni.ToLower().Contains(query))
				.Take(10)
				.Select(u => new UsuarioBuscadorDto
				{
					Id = u.Id,
					Nombre = u.Nombre,
					Dni = u.Dni
				})
				.ToListAsync();

			return resultados;
		}

		public async Task<List<UsuarioBuscadorDto>> ObtenerTodosLosJuradosAsync()
		{
			return await _dal.Query<Usuario>()
				.OfType<Jurado>()
				.Select(u => new UsuarioBuscadorDto
				{
					Id = u.Id,
					Nombre = u.Nombre,
					Dni = u.Dni
				})
				.ToListAsync();
		}

        public async Task<DashboardConcursanteDto> ObtenerDashboardConcursanteAsync(int usuarioId)
        {
            var concursante = await _dal.Query<Usuario>()
                .OfType<Concursante>()
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (concursante == null)
            {
                throw new KeyNotFoundException("Concursante no encontrado.");
            }

            var dashboard = new DashboardConcursanteDto
            {
                NombreUsuario = concursante.Nombre
            };

            // 1. Obtenemos los proyectos donde participa el concursante de forma directa
            var proyectos = await _dal.Query<Proyecto>()
                .Include(p => p.Evento)
                    .ThenInclude(e => e.votacion)
                .Where(p => p.Usuarios.Any(u => u.Id == usuarioId))
                .ToListAsync();

            foreach (var proy in proyectos)
            {
                if (proy == null || proy.Evento == null) continue;

                var estadoCalculado = DateTime.Now < proy.Evento.FechaInicio ? "PENDIENTE" : (DateTime.Now <= proy.Evento.FechaFin ? "ACTIVO" : "CERRADO");
                var votaciones = proy.Evento.votacion ?? new List<Votacion>();

                var proyectoDto = new ProyectoDashboardDto
                {
                    Id = proy.Id,
                    NombreProyecto = proy.Nombre,
                    EventoId = proy.Evento.Id,
                    NombreEvento = proy.Evento.Nombre,
                    EstadoEvento = estadoCalculado,
                    FechaInicioEvento = proy.Evento.FechaInicio,
                    FechaFinEvento = proy.Evento.FechaFin,
                    TotalVotaciones = votaciones.Count,
                    TotalComentarios = 0 // Lo calcularemos abajo
                };

                foreach (var votacion in votaciones)
                {
                    if (votacion == null) continue;

                    // 2. Extraemos los votos específicos de esta votación (cargando Proy y Comentarios)
                    var votosVotacion = await _dal.Query<Voto>()
                        .Include(v => v.Proy)
                        .Include(v => v.Comentarios)
                        .Where(v => v.Votacion.Id == votacion.Id)
                        .ToListAsync();

                    var votosDelProyecto = votosVotacion.Where(v => v.Proy != null && v.Proy.Id == proy.Id).ToList();
                    
                    var votosTotalesVotacion = votosVotacion
                        .Where(v => v.Proy != null)
                        .GroupBy(v => v.Proy.Id)
                        .Select(g => new { Id = g.Key, Puntuacion = g.Sum(x => x.Valoracion ?? 0) })
                        .OrderByDescending(x => x.Puntuacion)
                        .ToList();
                    
                    int? posicionProyecto = null;
                    if (votosTotalesVotacion.Any())
                    {
                        var puntajeProyecto = votosTotalesVotacion.FirstOrDefault(v => v.Id == proy.Id);
                        if (puntajeProyecto != null)
                        {
                            posicionProyecto = votosTotalesVotacion.IndexOf(puntajeProyecto) + 1;
                        }
                    }

                    var votacionDto = new VotacionDashboardDto
                    {
                        Id = votacion.Id,
                        Nombre = votacion.Nombre,
                        Estado = votacion.Estado.ToString(),
                        Puntuacion = votosDelProyecto.Sum(v => v.Valoracion ?? 0),
                        TotalVotos = votosDelProyecto.Count(),
                        Posicion = posicionProyecto
                    };

                    proyectoDto.Votaciones.Add(votacionDto);

                    foreach(var voto in votosDelProyecto)
                    {
                        var comentarios = voto.Comentarios ?? new List<Comentario>();
                        foreach(var comentario in comentarios)
                        {
                            if (comentario == null) continue;
                            proyectoDto.Comentarios.Add(new ComentarioDashboardDto
                            {
                                Autor = "JURADO",
                                FechaHora = comentario.FechaCreacion,
                                Contenido = comentario.Contenido,
                                Tipo = "COMENTARIO" 
                            });
                            proyectoDto.TotalComentarios++;
                        }
                    }

                    // Prems
                    var premio = await _dal.Query<Premio>().FirstOrDefaultAsync(p => p.VotacionId == votacion.Id);
                    if (premio != null)
                    {
                        bool esGanador = premio.ProyectoGanadorId == proy.Id || (posicionProyecto == 1 && (votacion.Estado.ToString().ToUpper() == "CERRADA" || votacion.Estado.ToString().ToUpper() == "FINALIZADA"));
                        
                        if (esGanador)
                        {
                            dashboard.Premios.Add(new PremioDashboardDto
                            {
                                Id = premio.Id,
                                Emoji = premio.Emoji,
                                NombrePremio = premio.Nombre,
                                NombreEvento = proy.Evento.Nombre,
                                NombreVotacion = votacion.Nombre,
                                Posicion = posicionProyecto,
                                Puntuacion = votacionDto.Puntuacion,
                                TipoGanador = posicionProyecto == 1 ? "1ª POSICIÓN" : "GANADOR ÚNICO"
                            });
                        }
                    }
                }

                dashboard.Proyectos.Add(proyectoDto);
            }

            dashboard.TotalProyectos = dashboard.Proyectos.Count;
            dashboard.TotalPremios = dashboard.Premios.Count;

            return dashboard;
        }

		public async Task CambiarContrasenyaAsync(CambiarContrasenyaDTO dto)
		{
			var usuario = await _dal.Query<Usuario>().FirstOrDefaultAsync(u => u.Dni == dto.Dni);
			if (usuario == null)
				throw new KeyNotFoundException("No existe ningún usuario registrado con ese DNI.");

			usuario.Contrasenya = dto.NuevaContrasenya;
			await _dal.CommitAsync();
		}
	}
}
