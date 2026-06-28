using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entitites;
using Domain.State;
using Persistance.Data;
using Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("Iniciando la siembra de la Base de Datos...");
            
            Console.WriteLine("Borrando la base de datos existente...");
            await db.Database.EnsureDeletedAsync();
            
            Console.WriteLine("Creando esquema de la base de datos...");
            await db.Database.EnsureCreatedAsync();
            db.EnsureSchemaCompat();

            Console.WriteLine("Insertando usuarios...");

            // 1. Admin
            var admin = new Administrador("Admin Principal", "11111111A", "123456");
            db.Users.Add(admin);

            // 2. Jurados (10)
            var jurados = new List<Jurado>();
            for (int i = 1; i <= 10; i++)
            {
                string digito = (i % 10).ToString();
                if (digito == "0") digito = "1";
                string dni = new string(digito[0], 8) + "J";
                if (i == 10) dni = "99999999J";
                var jurado = new Jurado($"Jurado {i}", dni, "123456");
                jurados.Add(jurado);
                db.Users.Add(jurado);
            }

            // 3. Concursantes (10)
            var concursantes = new List<Concursante>();
            for (int i = 1; i <= 10; i++)
            {
                string digito = (i % 10).ToString();
                if (digito == "0") digito = "1";
                string dni = new string(digito[0], 8) + "C";
                if (i == 10) dni = "99999999C";
                var concursante = new Concursante($"Concursante {i}", dni, "123456");
                concursantes.Add(concursante);
                db.Users.Add(concursante);
            }

            await db.SaveChangesAsync();

            var now = DateTime.UtcNow;
            
            var datosEventos = new[]
            {
                new {
                    Nombre = "Hackathon Global 2026",
                    Descripcion = "El mayor encuentro de desarrolladores creando soluciones para el mañana.",
                    EsEsports = false, Juego = "", Plataforma = "",
                    Proyectos = new[] { "EcoTrack AI", "HealthSync", "FinApp Plus", "EduStream VR", "SmartHome Hub", "Green AI" },
                    Votaciones = new[] { "Solución Más Ingeniosa", "Mejor Implementación Técnica", "Mejor Eficiencia de Código", "Mejor Diseño UI/UX" },
                    Comentarios = new[] { "El código está muy bien estructurado, pero falta optimizar las peticiones a la API.", "Una idea revolucionaria y la interfaz de usuario es impecable.", "El algoritmo es súper eficiente, gran trabajo de optimización.", "Buen prototipo, pero la escalabilidad parece ser un problema a largo plazo." }
                },
                new {
                    Nombre = "Festival de Eurovisión Universitario",
                    Descripcion = "Las mejores voces y composiciones del panorama universitario.",
                    EsEsports = false, Juego = "", Plataforma = "",
                    Proyectos = new[] { "Bajo la Lluvia (Pop)", "Ritmo Latino", "Voces del Alma", "Rock de Medianoche", "Electrónica Salvaje", "Acústico de Invierno" },
                    Votaciones = new[] { "Mejor Composición", "Mejor Ritmo y Mezcla", "Mejor Puesta en Escena", "Premio del Público Estudiantil" },
                    Comentarios = new[] { "La progresión de acordes en el estribillo me puso los pelos de punta. Magnífico.", "La mezcla de sonido ahogaba un poco la voz principal, pero el ritmo es súper pegadizo.", "Transmitieron muchísima energía en el escenario, imposible no bailar.", "La letra es muy profunda y la voz encaja perfectamente con el tono acústico." }
                },
                new {
                    Nombre = "Mundial de League of Legends 2026",
                    Descripcion = "Los mejores equipos compitiendo por la copa del invocador.",
                    EsEsports = true, Juego = "League of Legends", Plataforma = "PC",
                    Proyectos = new[] { "SKT T1", "G2 Esports", "Fnatic", "Cloud9", "T1", "FunPlus Phoenix" },
                    Votaciones = new[] { "Mejor Jugada del Torneo", "MVP (Most Valuable Player)", "Mejor Estrategia en Equipo", "Sorpresa del Campeonato" },
                    Comentarios = new[] { "El macrogame demostrado en esta partida ha sido de los mejores que he visto en años.", "Mecánicas individuales brutales, carreó al equipo entero en la teamfight del dragón.", "El draft fue un poco extraño, pero supieron ejecutar su condición de victoria a la perfección.", "Nadie se esperaba ese robo de Barón Nashor, la jugada del torneo sin duda." }
                },
                new {
                    Nombre = "Feria del Libro Indie",
                    Descripcion = "Premios a las mejores obras independientes del año.",
                    EsEsports = false, Juego = "", Plataforma = "",
                    Proyectos = new[] { "El Misterio del Reloj", "Sombras en la Noche", "Viaje a las Estrellas", "Poesía Urbana", "El Último Dragón", "Cuentos Cortos de Amor" },
                    Votaciones = new[] { "Mejor Trama Original", "Personaje Más Profundo", "Mejor Estilo Literario", "Portada Más Atractiva" },
                    Comentarios = new[] { "El desarrollo del protagonista principal es fascinante, te atrapa desde el primer capítulo.", "La narrativa es un poco lenta en el medio, pero el giro final lo compensa con creces.", "Tiene una prosa muy poética y fácil de leer al mismo tiempo. Gran pluma.", "Me encantó el diseño de la portada, refleja exactamente el tono misterioso de la historia." }
                },
                new {
                    Nombre = "Premios de Fotografía Natural",
                    Descripcion = "Celebrando la belleza de nuestro planeta a través del objetivo.",
                    EsEsports = false, Juego = "", Plataforma = "",
                    Proyectos = new[] { "Amanecer en los Alpes", "Tigre de Bengala", "El Vuelo del Águila", "Océano Profundo", "Macrofotografía de Insectos", "Aurora Boreal" },
                    Votaciones = new[] { "Mejor Composición Visual", "Mejor Uso de la Luz", "Captura Más Difícil", "Fotografía Más Emotiva" },
                    Comentarios = new[] { "El uso de la regla de los tercios y el enfoque en los ojos del animal es espectacular.", "La exposición es perfecta. Capturar tanta luz en esas condiciones requirió muchísima técnica.", "Transmite una paz inmensa. Es de esas fotos que podrías mirar durante horas.", "Un poco de ruido en las sombras, pero la rareza del momento capturado lo perdona todo." }
                },
                new {
                    Nombre = "Valorant Champions Tour 2026",
                    Descripcion = "El circuito competitivo más exigente de Valorant.",
                    EsEsports = true, Juego = "Valorant", Plataforma = "PC",
                    Proyectos = new[] { "Sentinels", "Paper Rex", "LOUD", "Team Liquid", "Leviatán", "KRÜ Esports" },
                    Votaciones = new[] { "Mejor Aim (Precisión)", "Mejor Uso de Habilidades", "Clutch del Año", "Mejor IGL (Líder del Juego)" },
                    Comentarios = new[] { "El control del recoil y el crosshair placement de este jugador están a otro nivel.", "Las rotaciones como equipo defensivo fueron rapidísimas, leyeron al enemigo perfectamente.", "Ese clutch 1v4 en la ronda de pistolas cambió por completo la inercia del mapa.", "El uso de las utilidades para ganar espacio en 'A' fue de manual, increíble sinergia." }
                },
                new {
                    Nombre = "Game Jam Anual 2026",
                    Descripcion = "Creación de videojuegos desde cero en 48 horas.",
                    EsEsports = false, Juego = "", Plataforma = "",
                    Proyectos = new[] { "Neon Rider", "Space Survivor", "Pixel Dungeon", "Cooking Chaos", "Detective Noir", "Mystery Puzzle" },
                    Votaciones = new[] { "Mejor Gameplay", "Arte Más Original", "Mejor Banda Sonora", "Diseño de Niveles Más Creativo" },
                    Comentarios = new[] { "El loop jugable es muy adictivo. Hice tres runs seguidas sin darme cuenta.", "Para haberse hecho en 48 horas, el apartado visual en pixel-art es de sobresaliente.", "Me he topado con un par de bugs de colisiones, pero la idea central es súper divertida.", "El diseño de niveles te enseña la mecánica principal sin necesidad de un tutorial aburrido." }
                },
                new {
                    Nombre = "Torneo de Super Smash Bros Ultimate",
                    Descripcion = "Los mejores jugadores de lucha se enfrentan en el escenario final.",
                    EsEsports = true, Juego = "Super Smash Bros Ultimate", Plataforma = "Nintendo Switch",
                    Proyectos = new[] { "Jugador MKLeo", "Jugador Spargo", "Jugador Tweek", "Jugador Glutonny", "Jugador Light", "Jugador Riddles" },
                    Votaciones = new[] { "Mejor Combo Realizado", "Mejor Recuperación", "Lectura del Oponente Más Épica", "Mejor Uso del Escenario" },
                    Comentarios = new[] { "Esa lectura del roll en el borde fue asquerosa. Demuestra un conocimiento del rival altísimo.", "El combo de 0 a muerte en la primera stock destruyó mentalmente a su oponente.", "El espaciado con los aéreos fue perfecto, no le dejó entrar en todo el match.", "Casi pierde el set por un SD (auto-destrucción) tonto, pero la remontada fue para la historia." }
                }
            };

            Votacion? votacionCerradaConVotos = null;
            Votacion? votacionActivaMitad = null;
            Votacion? votacionPendiente = null;
            Votacion? votacionActivaSinVotos = null;

            Console.WriteLine("Insertando eventos, votaciones, proyectos y votos...");

            var rng = new Random();

            for (int e = 0; e < datosEventos.Length; e++)
            {
                var datos = datosEventos[e];
                Evento evento;

                if (datos.EsEsports)
                {
                    evento = new EsportsEvento(datos.Nombre, datos.Descripcion, now.AddDays(-10), now.AddDays(10), datos.Juego, datos.Plataforma);
                }
                else
                {
                    evento = new EstandarEvento(datos.Nombre, datos.Descripcion, now.AddDays(-10), now.AddDays(10));
                }
                
                db.Events.Add(evento);

                // Proyectos para el evento
                var proyectos = new List<ProyectoEstandar>();
                for (int p = 0; p < datos.Proyectos.Length; p++)
                {
                    var proyecto = new ProyectoEstandar(datos.Proyectos[p], $"Descripción detallada de {datos.Proyectos[p]}.", evento);
                    proyecto.Usuarios.Add(concursantes[(e * 6 + p) % 10]);
                    db.Projects.Add(proyecto);
                    proyectos.Add(proyecto);
                }

                var premiosPosibles = new[] 
                {
                    new { Emoji = "👕", Nombre = "Camiseta del Evento" },
                    new { Emoji = "💳", Nombre = "Tarjeta Steam 20$" },
                    new { Emoji = "💳", Nombre = "Tarjeta Steam 100$" },
                    new { Emoji = "💻", Nombre = "Macbook Air" },
                    new { Emoji = "📱", Nombre = "iPad 128GB" },
                    new { Emoji = "🖥️", Nombre = "Monitor Gaming" },
                    new { Emoji = "🎧", Nombre = "Cascos Inalámbricos" },
                    new { Emoji = "⌨️", Nombre = "Teclado Mecánico" }
                };

                // 4 Votaciones por evento (alternando Votaciones Populares)
                var v1 = new Votacion(datos.Votaciones[0], $"Votación para {datos.Votaciones[0]}", now.AddDays(-10), now.AddDays(-5), 3, evento, EstadoComentarios.Opcionales, false);
                v1.SetEstado(new EstadoCerrada());
                
                var v2 = new Votacion(datos.Votaciones[1], $"Votación para {datos.Votaciones[1]}", now.AddDays(-2), now.AddDays(2), 3, evento, EstadoComentarios.Opcionales, true);
                v2.SetEstado(new EstadoActiva());
                
                var v3 = new Votacion(datos.Votaciones[2], $"Votación para {datos.Votaciones[2]}", now.AddDays(2), now.AddDays(10), 3, evento, EstadoComentarios.Opcionales, false);
                v3.SetEstado(new EstadoPendiente());
                
                var v4 = new Votacion(datos.Votaciones[3], $"Votación para {datos.Votaciones[3]}", now.AddDays(-1), now.AddDays(3), 3, evento, EstadoComentarios.Opcionales, true);
                v4.SetEstado(new EstadoActiva());

                // Asignar criterios de evaluación a algunas votaciones (V3 y V4)
                if (e % 2 == 0)
                {
                    v3.TipoVotacion = TipoVotacion.CriteriosPorPeso;
                    v3.ValorMaximoNumerico = 10;
                    v4.TipoVotacion = TipoVotacion.CriteriosPorPeso;
                    v4.ValorMaximoNumerico = 10;

                    if (datos.EsEsports)
                    {
                        v3.Criterios = new List<CriterioVotacion>
                        {
                            new CriterioVotacion { Nombre = "Mecánicas", Descripcion = "Habilidad individual", Peso = 0.40m, Votacion = v3 },
                            new CriterioVotacion { Nombre = "Estrategia", Descripcion = "Toma de decisiones", Peso = 0.40m, Votacion = v3 },
                            new CriterioVotacion { Nombre = "Creatividad", Descripcion = "Innovación táctica", Peso = 0.20m, Votacion = v3 }
                        };
                        v4.Criterios = new List<CriterioVotacion>
                        {
                            new CriterioVotacion { Nombre = "Comunicación", Descripcion = "Coordinación del equipo", Peso = 0.50m, Votacion = v4 },
                            new CriterioVotacion { Nombre = "Ejecución", Descripcion = "Precisión en jugadas", Peso = 0.50m, Votacion = v4 }
                        };
                    }
                    else
                    {
                         v3.Criterios = new List<CriterioVotacion>
                        {
                            new CriterioVotacion { Nombre = "Originalidad", Descripcion = "Nivel de innovación", Peso = 0.30m, Votacion = v3 },
                            new CriterioVotacion { Nombre = "Ejecución", Descripcion = "Calidad técnica", Peso = 0.40m, Votacion = v3 },
                            new CriterioVotacion { Nombre = "Presentación", Descripcion = "Comunicación visual", Peso = 0.30m, Votacion = v3 }
                        };
                         v4.Criterios = new List<CriterioVotacion>
                        {
                            new CriterioVotacion { Nombre = "Impacto", Descripcion = "Relevancia del proyecto", Peso = 0.50m, Votacion = v4 },
                            new CriterioVotacion { Nombre = "Diseño", Descripcion = "Estética y experiencia", Peso = 0.50m, Votacion = v4 }
                        };
                    }
                }

                var votaciones = new List<Votacion> { v1, v2, v3, v4 };

                foreach (var v in votaciones)
                {
                    // Asignar premios de forma aleatoria
                    var pAleatorio = premiosPosibles[rng.Next(premiosPosibles.Length)];
                    var premio = new Premio { Emoji = pAleatorio.Emoji, Nombre = pAleatorio.Nombre, Votacion = v };
                    db.Premios.Add(premio);

                    foreach(var j in jurados.Take(5))
                    {
                        v.JuradosAsignados.Add(j);
                    }
                    db.Votaciones.Add(v);
                }

                if (votacionCerradaConVotos == null) votacionCerradaConVotos = v1;
                if (votacionActivaMitad == null) votacionActivaMitad = v2;
                if (votacionPendiente == null) votacionPendiente = v3;
                if (votacionActivaSinVotos == null) votacionActivaSinVotos = v4;

                await db.SaveChangesAsync(); // Para tener IDs de Votaciones y Proyectos

                // Añadir votos y comentarios específicos del nicho
                // Votacion 1 (Cerrada) - Muchos votos
                foreach (var p in proyectos)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var jurado = jurados[j];
                        var voto = new Voto(now.AddDays(-6).AddHours(j), jurado, v1, p, rng.Next(1, 11));
                        db.Votes.Add(voto);
                        
                        // Añadir un comentario al voto seleccionado del array específico del evento
                        string textoComentario = datos.Comentarios[rng.Next(datos.Comentarios.Length)];
                        var comentario = new Comentario(textoComentario, now.AddDays(-6).AddHours(j).AddMinutes(5), voto);
                        db.Comments.Add(comentario);
                    }
                }

                // Votacion 2 (Activa Mitad) - Algunos votos
                for (int j = 0; j < 2; j++)
                {
                    var jurado = jurados[j];
                    var voto = new Voto(now.AddDays(-1).AddHours(j), jurado, v2, proyectos[0], rng.Next(1, 11));
                    db.Votes.Add(voto);

                    string textoComentario = datos.Comentarios[rng.Next(datos.Comentarios.Length)];
                    var comentario = new Comentario(textoComentario, now.AddDays(-1).AddHours(j).AddMinutes(5), voto);
                    db.Comments.Add(comentario);
                }
            }

            await db.SaveChangesAsync();

            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("SIEMBRA DE DATOS COMPLETADA CON ÉXITO.");
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("Para ubicarte fácilmente, puedes revisar estas votaciones:");
            Console.WriteLine($"- Votación Cerrada con Votos:      '{votacionCerradaConVotos?.Nombre}' (Evento: {votacionCerradaConVotos?.evento?.Nombre})");
            Console.WriteLine($"- Votación Activa a Mitad de Votar:'{votacionActivaMitad?.Nombre}' (Evento: {votacionActivaMitad?.evento?.Nombre})");
            Console.WriteLine($"- Votación Pendiente:              '{votacionPendiente?.Nombre}' (Evento: {votacionPendiente?.evento?.Nombre})");
            Console.WriteLine($"- Votación Activa Sin Votos:       '{votacionActivaSinVotos?.Nombre}' (Evento: {votacionActivaSinVotos?.evento?.Nombre})");
            Console.WriteLine("-------------------------------------------------------------------");
        }
    }
}
