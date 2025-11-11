using GestionLlaves.Controllers;
using GestionLlaves.Data;
using GestionLlaves.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace GestionLlaves.Tests.Controllers
{
    public class AccountControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private byte[] HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        [Fact]
        public async Task Login_ConCredencialesValidas_DeberiaRedirigirADashboard()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            
            // Crear usuario de prueba
            var persona = new Persona
            {
                Id = 1,
                Nombres = "Juan",
                PrimerApellido = "Pérez",
                Estado = true,
                Tipo = "DOCENTE"
            };
            context.Persona.Add(persona);

            var usuario = new Usuario
            {
                Id = 1,
                Email = "juan.perez@univalle.edu.bo",
                Contrasenia = HashPassword("Password123"),
                Rol = "DOCENTE",
                Estado = true,
                FechaUltimaConexion = DateTime.Now.AddDays(-1),
                CreadoModPor = 1
            };
            context.Usuario.Add(usuario);
            await context.SaveChangesAsync();

            var controller = new AccountController(context);
            var httpContext = new DefaultHttpContext();
            var sessionStore = new Dictionary<string, byte[]>();
            var session = new Mock<ISession>();
            session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => sessionStore[key] = value);
            session.Setup(s => s.Get(It.IsAny<string>()))
                .Returns<string>(key => sessionStore.TryGetValue(key, out var value) ? value : null);
            httpContext.Session = session.Object;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.Login("juan.perez@univalle.edu.bo", "Password123");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Docente", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_ConCredencialesInvalidas_DeberiaRetornarViewConMensajeError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AccountController(context);

            // Act
            var result = await controller.Login("usuario@inexistente.com", "PasswordIncorrecto");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Mensaje);
            Assert.Contains("incorrectos", controller.ViewBag.Mensaje.ToString().ToLower());
        }

        [Fact]
        public async Task Login_ConCamposVacios_DeberiaRetornarViewConMensajeError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AccountController(context);

            // Act
            var result = await controller.Login("", "");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Mensaje);
            Assert.Contains("complete", controller.ViewBag.Mensaje.ToString().ToLower());
        }

        [Fact]
        public async Task Login_UsuarioInactivo_DeberiaRetornarViewConMensajeError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            
            var persona = new Persona
            {
                Id = 2,
                Nombres = "María",
                PrimerApellido = "González",
                Estado = true,
                Tipo = "DOCENTE"
            };
            context.Persona.Add(persona);

            var usuario = new Usuario
            {
                Id = 2,
                Email = "maria.gonzalez@univalle.edu.bo",
                Contrasenia = HashPassword("Password123"),
                Rol = "DOCENTE",
                Estado = false, // Usuario inactivo
                FechaUltimaConexion = DateTime.Now.AddDays(-1),
                CreadoModPor = 1
            };
            context.Usuario.Add(usuario);
            await context.SaveChangesAsync();

            var controller = new AccountController(context);

            // Act
            var result = await controller.Login("maria.gonzalez@univalle.edu.bo", "Password123");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Mensaje);
        }

        [Fact]
        public async Task CambiarPasswordPrimeraVez_ConDatosValidos_DeberiaActualizarPassword()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            
            var persona = new Persona
            {
                Id = 3,
                Nombres = "Carlos",
                PrimerApellido = "López",
                Estado = true,
                Tipo = "DOCENTE"
            };
            context.Persona.Add(persona);

            var passwordOriginal = HashPassword("PasswordOriginal");
            var usuario = new Usuario
            {
                Id = 3,
                Email = "carlos.lopez@univalle.edu.bo",
                Contrasenia = passwordOriginal,
                Rol = "DOCENTE",
                Estado = true,
                FechaUltimaConexion = null, // Primera vez
                CreadoModPor = 1
            };
            context.Usuario.Add(usuario);
            await context.SaveChangesAsync();

            var controller = new AccountController(context);

            // Act
            var result = await controller.CambiarPasswordPrimeraVez(
                "carlos.lopez@univalle.edu.bo",
                "PasswordOriginal",
                "NuevaPassword123",
                "NuevaPassword123"
            );

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);

            // Verificar que la contraseña fue actualizada
            var usuarioActualizado = await context.Usuario.FindAsync(3);
            Assert.NotNull(usuarioActualizado);
            Assert.NotNull(usuarioActualizado.FechaUltimaConexion);
            Assert.NotEqual(passwordOriginal, usuarioActualizado.Contrasenia);
        }

        [Fact]
        public async Task CambiarPasswordPrimeraVez_ConPasswordActualIncorrecta_DeberiaRetornarError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            
            var persona = new Persona
            {
                Id = 4,
                Nombres = "Ana",
                PrimerApellido = "Martínez",
                Estado = true,
                Tipo = "DOCENTE"
            };
            context.Persona.Add(persona);

            var usuario = new Usuario
            {
                Id = 4,
                Email = "ana.martinez@univalle.edu.bo",
                Contrasenia = HashPassword("PasswordCorrecto"),
                Rol = "DOCENTE",
                Estado = true,
                FechaUltimaConexion = null,
                CreadoModPor = 1
            };
            context.Usuario.Add(usuario);
            await context.SaveChangesAsync();

            var controller = new AccountController(context);

            // Act
            var result = await controller.CambiarPasswordPrimeraVez(
                "ana.martinez@univalle.edu.bo",
                "PasswordIncorrecto",
                "NuevaPassword123",
                "NuevaPassword123"
            );

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Mensaje);
            Assert.Contains("incorrecta", controller.ViewBag.Mensaje.ToString().ToLower());
        }

        [Fact]
        public void Logout_DeberiaLimpiarSesionYRedirigirALogin()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AccountController(context);
            var httpContext = new DefaultHttpContext();
            var session = new Mock<ISession>();
            httpContext.Session = session.Object;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }
    }
}

