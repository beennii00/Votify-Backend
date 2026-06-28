using Microsoft.EntityFrameworkCore;
using Domain.Entitites;
using Domain.Factory;
using Domain.Factory.Usuarios;
using Persistance.Data;
using Persistance.Repositories;
using Shared.Enums;

namespace TestProject_xUnit
{
    public class IntegracionPostgresTest : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IDAL _dal;

        public IntegracionPostgresTest()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=VotifyDb;Username=postgres;Password=1234")
                .Options;

            _context = new AppDbContext(options);
            _dal = new EntityFrameworkDAL(_context);

            // Limpiar la BD antes de cada test para garantizar aislamiento
            _context.RemoveAllData();
        }

        // =====================================================================
        // TEST 1 — Poblar la BD con todas las entidades y verificar que existen
        // =====================================================================
        [Fact]
        public void Test1_PoblarTodasLasEntidades_DebeGuardarCorrectamente()
        {
            // ARRANGE & ACT
            var (evento, proyecto, votacion, usuario, voto, comentario) = CrearDatosCompletos("T1");

            // ASSERT — existen en BD
            Assert.True(_dal.Exists<Evento>(evento.Id));
            Assert.True(_dal.Exists<Proyecto>(proyecto.Id));
            Assert.True(_dal.Exists<Votacion>(votacion.Id));
            Assert.True(_dal.Exists<Usuario>(usuario.Id));
            Assert.True(_dal.Exists<Voto>(voto.Id));
            Assert.True(_dal.Exists<Comentario>(comentario.Id));

            // ASSERT — los IDs fueron generados por Postgres (> 0)
            Assert.True(evento.Id > 0);
            Assert.True(proyecto.Id > 0);
            Assert.True(votacion.Id > 0);
            Assert.True(usuario.Id > 0);
            Assert.True(voto.Id > 0);
            Assert.True(comentario.Id > 0);
        }

        // =====================================================================
        // TEST 2 — Modificar entidades y verificar que los cambios persisten
        // =====================================================================
        [Fact]
        public void Test2_ModificarEntidades_DebeReflejarCambiosEnBD()
        {
            // ARRANGE
            var (evento, proyecto, votacion, usuario, voto, comentario) = CrearDatosCompletos("T2");

            // ACT — modificar nombre del evento
            var eventoDb = _dal.GetById<Evento>(evento.Id);
            eventoDb.Nombre = "Hackathon 2025 EDITADO";
            eventoDb.Descripcion = "Descripcion actualizada";
            _dal.Commit();

            // ACT  modificar el usuario
            var usuarioDb = _dal.GetById<Usuario>(usuario.Id);
            usuarioDb.Nombre = "nuevo_nombre";
            usuarioDb.Dni = "00000000X";
            _dal.Commit();

            // ACT  modificar el contenido del comentario
            var comentarioDb = _dal.GetById<Comentario>(comentario.Id);
            comentarioDb.Contenido = "Comentario actualizado tras revision";
            _dal.Commit();

            // ASSERT — recuperar de nuevo y verificar cambios
            var eventoActualizado = _dal.GetById<Evento>(evento.Id);
            Assert.Equal("Hackathon 2025 EDITADO", eventoActualizado.Nombre);
            Assert.Equal("Descripcion actualizada", eventoActualizado.Descripcion);

            var usuarioActualizado = _dal.GetById<Usuario>(usuario.Id);
            Assert.Equal("nuevo_nombre", usuarioActualizado.Nombre);
            Assert.Equal("00000000X", usuarioActualizado.Dni);

            var comentarioActualizado = _dal.GetById<Comentario>(comentario.Id);
            Assert.Equal("Comentario actualizado tras revision", comentarioActualizado.Contenido);
        }

        // =====================================================================
        // TEST 3 — Agregar mas entidades relacionadas y verificar colecciones
        // =====================================================================
        [Fact]
        public void Test3_AgregarMasEntidadesRelacionadas_DebeAcumularseEnBD()
        {
            // ARRANGE — datos base
            var (evento, proyecto, votacion, usuario, voto, _) = CrearDatosCompletos("T3");

            // ACT  agregar un segundo usuario
            UsuarioCreator creadorUsuario = new ConcursanteCreator();
            var usuario2 = creadorUsuario.CreateUsuario("Laura", "11111111A", "pass456");
            _dal.Insert(usuario2);
            _dal.Commit();

            // ACT  agregar un segundo proyecto al mismo evento
            // ==================================
            // 2. CREAR PROYECTO
            // ==================================
            var creadorProyecto = new ProyectoEstandarCreator();
            var proyecto2 = creadorProyecto.CreateProyecto("VotifyMobile", "Version mobile de Votify", evento);
            _dal.Insert(proyecto2);
            _dal.Commit();

            // ACT — segundo usuario vota al segundo proyecto
            var voto2 = new Voto(DateTime.UtcNow, usuario2, votacion, proyecto2);
            _dal.Insert(voto2);
            _dal.Commit();

            // ACT — comentario del segundo voto
            var comentario2 = new Comentario("Tambien muy buen proyecto!", DateTime.UtcNow, voto2);
            _dal.Insert(comentario2);
            _dal.Commit();

            // ACT  tercer usuario sin voto todavia
            var usuario3 = creadorUsuario.CreateUsuario("Pedro", "22222222B", "pass789");
            _dal.Insert(usuario3);
            _dal.Commit();

            // ASSERT  verificar cantidades acumuladas
            var todosUsuarios = _dal.GetAll<Usuario>().ToList();
            Assert.Equal(3, todosUsuarios.Count);

            var todosProyectos = _dal.GetAll<Proyecto>().ToList();
            Assert.Equal(2, todosProyectos.Count);

            var todosVotos = _dal.GetAll<Voto>().ToList();
            Assert.Equal(2, todosVotos.Count);

            var todosComentarios = _dal.GetAll<Comentario>().ToList();
            Assert.Equal(2, todosComentarios.Count);
        }

        // =====================================================================
        // TEST 4 — Filtrar con GetWhere y consultas especificas
        // =====================================================================
        [Fact]
        public void Test4_FiltrarConGetWhere_DebeRetornarResultadosCorrectos()
        {
            // ARRANGE
            var (evento, proyecto, votacion, usuario, _, _) = CrearDatosCompletos("T4");

            // Agregar usuarios adicionales para poder filtrar
            UsuarioCreator creadorUsuario = new ConcursanteCreator();
            var usuario2 = creadorUsuario.CreateUsuario("Ana", "33333333C", "pass");
            var usuario3 = creadorUsuario.CreateUsuario("Luis", "44444444D", "pass");
            _dal.Insert(usuario2);
            _dal.Insert(usuario3);
            _dal.Commit();

            // Agregar votos adicionales en distintas fechas
            var votoAntiguo = new Voto(DateTime.UtcNow.AddDays(-10), usuario2, votacion, proyecto);
            var votoReciente = new Voto(DateTime.UtcNow.AddDays(-1), usuario3, votacion, proyecto);
            _dal.Insert(votoAntiguo);
            _dal.Insert(votoReciente);
            _dal.Commit();

            // ACT  filtrar usuarios por DNI
            var usuariosFiltrados = _dal.GetWhere<Usuario>(u => u.Dni.EndsWith("D") || u.Dni.EndsWith("C")).ToList();

            // ACT  filtrar votos de los ultimos 5 dias
            // El voto de CrearDatosCompletos (hoy) + votoReciente (ayer) = 2 votos dentro del rango
            var votosRecientes = _dal.GetWhere<Voto>(v => v.FechaVotado >= DateTime.UtcNow.AddDays(-5)).ToList();

            // ACT  buscar evento por nombre exacto
            var eventoFiltrado = _dal.GetWhere<Evento>(e => e.Nombre == "Hackathon T4").FirstOrDefault();

            // ASSERT
            Assert.Equal(2, usuariosFiltrados.Count); // Ana y Luis
            Assert.Equal(2, votosRecientes.Count); // voto de CrearDatosCompletos (hoy) + votoReciente (ayer)
            Assert.NotNull(eventoFiltrado);
            Assert.Equal("Hackathon T4", eventoFiltrado.Nombre);
        }

        // =====================================================================
        // TEST 5 — Eliminar entidades y verificar que desaparecen
        // =====================================================================
        [Fact]
        public void Test5_EliminarEntidades_DebeQuitarlasDesBD()
        {
            // ARRANGE
            var (_, proyecto, votacion, usuario, voto, comentario) = CrearDatosCompletos("T5");

            var comentarioId = comentario.Id;
            var votoId = voto.Id;

            // ACT — eliminar comentario primero (depende de voto)
            var comentarioDb = _dal.GetById<Comentario>(comentarioId);
            _dal.Delete(comentarioDb);
            _dal.Commit();

            // ACT — eliminar voto
            var votoDb = _dal.GetById<Voto>(votoId);
            _dal.Delete(votoDb);
            _dal.Commit();

            // ASSERT — ya no existen
            Assert.False(_dal.Exists<Comentario>(comentarioId));
            Assert.False(_dal.Exists<Voto>(votoId));

            // ASSERT — el resto sigue intacto
            Assert.True(_dal.Exists<Usuario>(usuario.Id));
            Assert.True(_dal.Exists<Proyecto>(proyecto.Id));
            Assert.True(_dal.Exists<Votacion>(votacion.Id));
        }

        // =====================================================================
        // TEST 6 — Transaccion con CommitTransaction exitoso
        // =====================================================================
        [Fact]
        public void Test6_TransaccionExitosa_DebeGuardarTodo()
        {
            // ARRANGE
            var creador = new EstandarEventoCreator();
            var evento = creador.CreateEvento("Evento Transaccion T6", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(5));

            // ACT
            _dal.BeginTransaction();
            _dal.Insert(evento);
            _dal.Commit();

            var creadorProyecto2 = new ProyectoEstandarCreator();
            var proyecto = creadorProyecto2.CreateProyecto("Proyecto Transaccion T6", "Desc", evento);
            _dal.Insert(proyecto);
            _dal.Commit();

            _dal.CommitTransaction();

            // ASSERT — ambos persisten
            Assert.True(_dal.Exists<Evento>(evento.Id));
            Assert.True(_dal.Exists<Proyecto>(proyecto.Id));
        }

        // =====================================================================
        // TEST 7 — Transaccion con RollbackTransaction debe deshacer cambios
        // =====================================================================
        [Fact]
        public void Test7_TransaccionConRollback_DebeRevertirCambios()
        {
            // ARRANGE  insertar un evento fuera de transaccion para having it as a reference
            var creador = new EstandarEventoCreator();
            var eventoBase = creador.CreateEvento("Evento Base T7", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(5));
            _dal.Insert(eventoBase);
            _dal.Commit();
            var eventoBaseId = eventoBase.Id;

            // ACT  iniciar transaccion, insertar y hacer rollback
            _dal.BeginTransaction();
            var eventoRollback = creador.CreateEvento("Evento Rollback T7", "No deberia persistir", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
            _dal.Insert(eventoRollback);
            _dal.Commit(); // guarda en contexto dentro de la transaccion
            _dal.RollbackTransaction(); // deshace la transaccion en BD

            // ASSERT — el evento base sigue ahi
            Assert.True(_dal.Exists<Evento>(eventoBaseId));

            // ASSERT — el evento del rollback no existe en BD
            var eventosT7 = _dal.GetWhere<Evento>(e => e.Nombre == "Evento Rollback T7").ToList();
            Assert.Empty(eventosT7);
        }

        // =====================================================================
        // TEST 8 — Clear de una entidad borra todos sus registros
        // =====================================================================
        [Fact]
        public void Test8_ClearEntidad_DebeEliminarTodosLosRegistros()
        {
            // ARRANGE  insertar varios eventos
            var creador = new EstandarEventoCreator();
            var evento1 = creador.CreateEvento("Evento Clear 1 T8", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
            var evento2 = creador.CreateEvento("Evento Clear 2 T8", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(2));
            var evento3 = creador.CreateEvento("Evento Clear 3 T8", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(3));
            _dal.Insert(evento1);
            _dal.Insert(evento2);
            _dal.Insert(evento3);
            _dal.Commit();

            var totalAntes = _dal.GetAll<Evento>().Count();
            Assert.Equal(3, totalAntes);

            // ACT
            _dal.Clear<Evento>();
            _dal.Commit();

            // ASSERT
            var totalDespues = _dal.GetAll<Evento>().Count();
            Assert.Equal(0, totalDespues);
        }

        // =====================================================================
        // HELPER — crea un juego completo de datos con un sufijo para distinguir
        // =====================================================================
        private (Evento, Proyecto, Votacion, Usuario, Voto, Comentario) CrearDatosCompletos(string sufijo)
        {
            var creador = new EstandarEventoCreator();
            var evento = creador.CreateEvento(
                $"Hackathon {sufijo}",
                $"Evento de votacion {sufijo}",
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(7)
            );
            _dal.Insert(evento);
            _dal.Commit();

            var creadorProyecto3 = new ProyectoEstandarCreator();
            var proyecto = creadorProyecto3.CreateProyecto($"Proyecto {sufijo}", $"Descripcion {sufijo}", evento);
            _dal.Insert(proyecto);
            _dal.Commit();

            var votacion = new Votacion($"Votacion {sufijo}", "Descripcin test", DateTime.UtcNow, DateTime.UtcNow.AddDays(3), 3, evento, Shared.Enums.EstadoComentarios.Opcionales, false);
            _dal.Insert(votacion);
            _dal.Commit();

            UsuarioCreator creadorUsuario = new ConcursanteCreator();
            var usuario = creadorUsuario.CreateUsuario($"Usuario {sufijo}", $"DNI{sufijo}", "pass");
            _dal.Insert(usuario);
            _dal.Commit();

            var voto = new Voto(DateTime.UtcNow, usuario, votacion, proyecto);
            _dal.Insert(voto);
            _dal.Commit();

            var comentario = new Comentario($"Comentario {sufijo}", DateTime.UtcNow, voto);
            _dal.Insert(comentario);
            _dal.Commit();

            return (evento, proyecto, votacion, usuario, voto, comentario);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
