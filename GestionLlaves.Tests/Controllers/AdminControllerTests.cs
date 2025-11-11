using GestionLlaves.Controllers;
using GestionLlaves.Data;
using GestionLlaves.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GestionLlaves.Tests.Controllers
{
    public class AdminControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private void SetupAdminSession(AdminController controller)
        {
            var httpContext = new DefaultHttpContext();
            var sessionStore = new Dictionary<string, byte[]>();
            var session = new Mock<ISession>();
            
            session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => sessionStore[key] = value);
            session.Setup(s => s.GetString(It.IsAny<string>()))
                .Returns<string>(key => 
                    key == "UsuarioRol" ? "ADMIN" : 
                    key == "UsuarioEmail" ? "admin@test.com" : null);
            
            httpContext.Session = session.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Fact]
        public void Index_ConAdminAutenticado_DeberiaRetornarView()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AdminController(context);
            SetupAdminSession(controller);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Index_ConUsuarioNoAutenticado_DeberiaRedirigirALogin()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AdminController(context);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task MarcarDevuelto_ConPrestamoValido_DeberiaActualizarEstado()
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
                FechaInicio = DateTime.Now.AddHours(-2),
                FechaFinProgramada = DateTime.Now.AddHours(-1),
                EstadoPrestamo = "ACTIVO",
                Tipo = "REGULAR",
                Estado = true,
                CreadoModPor = 1
            };
            context.Prestamo.Add(prestamo);
            await context.SaveChangesAsync();

            var controller = new AdminController(context);
            SetupAdminSession(controller);

            // Act
            var result = controller.MarcarDevuelto(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            var prestamoActualizado = await context.Prestamo.FindAsync(1);
            Assert.NotNull(prestamoActualizado);
            Assert.NotNull(prestamoActualizado.FechaFinReal);
        }

        [Fact]
        public async Task CambiarEstadoSolicitud_ConSolicitudValida_DeberiaActualizarEstado()
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
                FechaInicio = DateTime.Now,
                FechaFinProgramada = DateTime.Now.AddHours(2),
                EstadoPrestamo = "ACTIVO",
                Tipo = "REGULAR",
                Estado = true,
                CreadoModPor = 1
            };
            context.Prestamo.Add(prestamo);
            await context.SaveChangesAsync();

            var controller = new AdminController(context);
            SetupAdminSession(controller);

            // Act
            var result = controller.CambiarEstadoSolicitud(1, "APROBADO");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            var prestamoActualizado = await context.Prestamo.FindAsync(1);
            Assert.Equal("APROBADO", prestamoActualizado.EstadoPrestamo);
        }

        [Fact]
        public async Task RegistrarUsuario_ConDatosValidos_DeberiaCrearUsuario()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AdminController(context);
            SetupAdminSession(controller);

            // Act
            var result = controller.RegistrarUsuario(
                "María", "González", "López", "12345678", "DOCENTE", "maria@test.com", "DOCENTE");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var usuarioCreado = await context.Usuario.FirstOrDefaultAsync(u => u.Email == "maria@test.com");
            Assert.NotNull(usuarioCreado);
        }

        [Fact]
        public async Task RegistrarUsuario_ConEmailDuplicado_DeberiaRetornarError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var persona = new Persona
            {
                Id = 1, Nombres = "Juan", PrimerApellido = "Pérez", Estado = true, Tipo = "DOCENTE", CreadoModPor = 1
            };
            context.Persona.Add(persona);
            var usuario = new Usuario
            {
                Id = 1, Email = "juan@test.com", Rol = "DOCENTE", Estado = true, CreadoModPor = 1
            };
            context.Usuario.Add(usuario);
            await context.SaveChangesAsync();

            var controller = new AdminController(context);
            SetupAdminSession(controller);

            // Act
            var result = controller.RegistrarUsuario(
                "María", "González", "", "12345678", "DOCENTE", "juan@test.com", "DOCENTE");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Mensaje);
            Assert.Contains("ya está registrado", controller.ViewBag.Mensaje.ToString());
        }
    }
}

