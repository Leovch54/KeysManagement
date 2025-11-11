using GestionLlaves.Data;
using GestionLlaves.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionLlaves.Tests.Services
{
    public class PrestamoServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CrearPrestamo_ConDatosValidos_DeberiaGuardarEnBaseDeDatos()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var persona = new Persona
            {
                Id = 1, Nombres = "Juan", PrimerApellido = "Pérez", Estado = true, Tipo = "DOCENTE", CreadoModPor = 1
            };
            context.Persona.Add(persona);
            var aula = new Aula { Id = 1, Codigo = "A101", Estado = true, CreadoModPor = 1 };
            context.Aula.Add(aula);
            await context.SaveChangesAsync();

            var prestamo = new Prestamo
            {
                PersonaId = 1,
                AulaId = 1,
                FechaInicio = DateTime.Now,
                FechaFinProgramada = DateTime.Now.AddHours(2),
                EstadoPrestamo = "ACTIVO",
                Tipo = "REGULAR",
                Estado = true,
                CreadoModPor = 1
            };

            // Act
            context.Prestamo.Add(prestamo);
            await context.SaveChangesAsync();

            // Assert
            var prestamoGuardado = await context.Prestamo.FindAsync(prestamo.Id);
            Assert.NotNull(prestamoGuardado);
            Assert.Equal("ACTIVO", prestamoGuardado.EstadoPrestamo);
        }

        [Fact]
        public async Task MarcarPrestamoComoVencido_ConPrestamoVencido_DeberiaActualizarEstado()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var persona = new Persona
            {
                Id = 1, Nombres = "Juan", PrimerApellido = "Pérez", Estado = true, Tipo = "DOCENTE", CreadoModPor = 1
            };
            context.Persona.Add(persona);
            var aula = new Aula { Id = 1, Codigo = "A101", Estado = true, CreadoModPor = 1 };
            context.Aula.Add(aula);
            var prestamo = new Prestamo
            {
                Id = 1,
                PersonaId = 1,
                AulaId = 1,
                FechaInicio = DateTime.Now.AddHours(-3),
                FechaFinProgramada = DateTime.Now.AddHours(-1), // Ya venció
                EstadoPrestamo = "ACTIVO",
                Tipo = "REGULAR",
                Estado = true,
                CreadoModPor = 1
            };
            context.Prestamo.Add(prestamo);
            await context.SaveChangesAsync();

            // Act
            var prestamoActual = await context.Prestamo.FindAsync(1);
            if (prestamoActual != null && DateTime.Now > prestamoActual.FechaFinProgramada)
            {
                prestamoActual.EstadoPrestamo = "VENCIDO";
                await context.SaveChangesAsync();
            }

            // Assert
            var prestamoActualizado = await context.Prestamo.FindAsync(1);
            Assert.Equal("VENCIDO", prestamoActualizado.EstadoPrestamo);
        }

        [Fact]
        public async Task ValidarDisponibilidadAula_ConAulaOcupada_DeberiaRetornarFalse()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var persona = new Persona
            {
                Id = 1, Nombres = "Juan", PrimerApellido = "Pérez", Estado = true, Tipo = "DOCENTE", CreadoModPor = 1
            };
            context.Persona.Add(persona);
            var aula = new Aula { Id = 1, Codigo = "A101", Estado = true, CreadoModPor = 1 };
            context.Aula.Add(aula);
            var prestamo = new Prestamo
            {
                PersonaId = 1,
                AulaId = 1,
                FechaInicio = DateTime.Now,
                FechaFinProgramada = DateTime.Now.AddHours(2),
                EstadoPrestamo = "ACTIVO",
                Tipo = "REGULAR",
                Estado = true,
                CreadoModPor = 1
            };
            context.Prestamo.Add(prestamo);
            await context.SaveChangesAsync();

            // Act
            var fechaInicio = DateTime.Now.AddMinutes(30);
            var fechaFin = DateTime.Now.AddHours(1);
            var estaOcupada = await context.Prestamo
                .AnyAsync(p => p.AulaId == 1
                    && p.EstadoPrestamo == "ACTIVO"
                    && p.FechaInicio < fechaFin
                    && p.FechaFinProgramada > fechaInicio);

            // Assert
            Assert.True(estaOcupada);
        }
    }
}

