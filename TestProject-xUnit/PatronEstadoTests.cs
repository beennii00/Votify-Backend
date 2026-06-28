using System;
using Domain.Entitites;
using Domain.State;
using Shared.Enums;
using Xunit;

namespace TestProject_xUnit
{
    public class PatronEstadoTests
    {
        // ── Helper para crear Votacion "hueca" sin necesidad de Base de Datos ──
        private static Votacion CrearVotacionDesdeCero(EstadoVotacion estadoInicial, IEstadoVotacion claseEstado)
        {
            // Creamos instancia hueca del contexto
            var vot = new Votacion
            {
                Nombre = "Votación de prueba",
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddHours(2),
                evento = new EstandarEvento("Evento falso", "Desc", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1))
            };
            
            // Forzamos el estado de partida.
            vot.SetEstado(claseEstado);
            return vot;
        }

        // ── Tests de Transiciones Válidas (Happy Path) ────────────────

        [Fact]
        public void Iniciar_DesdePendiente_CambiaAActiva_ModificaFechaInicioYEnum()
        {
            // Arrange
            var votacion = CrearVotacionDesdeCero(EstadoVotacion.Pendiente, new EstadoPendiente());

            // Act
            votacion.Iniciar();

            // Assert
            Assert.Equal(EstadoVotacion.Activa, votacion.Estado);
            Assert.True(votacion.FechaInicio <= DateTime.UtcNow);
        }

        [Fact]
        public void Pausar_DesdeActiva_CambiaAPausada()
        {
            // Arrange
            var votacion = CrearVotacionDesdeCero(EstadoVotacion.Activa, new EstadoActiva());

            // Act
            votacion.Pausar();

            // Assert
            Assert.Equal(EstadoVotacion.Pausada, votacion.Estado);
        }

        [Fact]
        public void Reanudar_DesdePausada_CambiaAActiva()
        {
            // Arrange
            var votacion = CrearVotacionDesdeCero(EstadoVotacion.Pausada, new EstadoPausada());

            // Act
            votacion.Reanudar();

            // Assert
            Assert.Equal(EstadoVotacion.Activa, votacion.Estado);
        }

        [Fact]
        public void Cerrar_DesdeActiva_CambiaACerrada_YModificaFechaFin()
        {
            // Arrange
            var votacion = CrearVotacionDesdeCero(EstadoVotacion.Activa, new EstadoActiva());

            // Act
            votacion.Cerrar();

            // Assert
            Assert.Equal(EstadoVotacion.Cerrada, votacion.Estado);
            Assert.True(votacion.FechaFin <= DateTime.UtcNow);
        }

        // ── Tests de Transiciones Inválidas (Excepciones) ─────────────

        [Fact]
        public void Pausar_DesdePendiente_LanzaExcepcion()
        {
            // Arrange
            var votacion = CrearVotacionDesdeCero(EstadoVotacion.Pendiente, new EstadoPendiente());

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => votacion.Pausar());
            Assert.Contains("No se puede pausar una votación pendiente", ex.Message);
        }

        [Fact]
        public void Reanudar_DesdeActiva_LanzaExcepcion()
        {
            // Arrange
            var votacion = CrearVotacionDesdeCero(EstadoVotacion.Activa, new EstadoActiva());

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => votacion.Reanudar());
            Assert.Contains("La votación ya está activa", ex.Message);
        }

        [Fact]
        public void Iniciar_DesdeCerrada_LanzaExcepcion()
        {
            // Arrange
            var votacion = CrearVotacionDesdeCero(EstadoVotacion.Cerrada, new EstadoCerrada());

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => votacion.Iniciar());
            Assert.Contains("No se puede iniciar una votación cerrada", ex.Message);
        }
    }
}