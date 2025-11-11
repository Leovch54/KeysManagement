using GestionLlaves.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace GestionLlaves.Tests.Models
{
    public class UsuarioTests
    {
        [Fact]
        public void Usuario_EmailInvalido_DeberiaFallarValidacion()
        {
            // Arrange
            var usuario = new Usuario
            {
                Email = "email-invalido",
                Rol = "DOCENTE"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(usuario);
            var isValid = Validator.TryValidateObject(usuario, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
        }

        [Fact]
        public void Usuario_EmailValido_DeberiaPasarValidacion()
        {
            // Arrange
            var usuario = new Usuario
            {
                Email = "usuario@univalle.edu.bo",
                Rol = "DOCENTE"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(usuario);
            var isValid = Validator.TryValidateObject(usuario, validationContext, validationResults, true);

            // Assert
            // Nota: Puede fallar si faltan campos requeridos, pero el email debe ser válido
            var emailErrors = validationResults.Where(v => v.MemberNames.Contains("Email"));
            Assert.Empty(emailErrors);
        }

        [Fact]
        public void Usuario_RolInvalido_DeberiaFallarValidacion()
        {
            // Arrange
            var usuario = new Usuario
            {
                Email = "usuario@univalle.edu.bo",
                Rol = "ROL_INVALIDO"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(usuario);
            var isValid = Validator.TryValidateObject(usuario, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Rol"));
        }

        [Fact]
        public void Usuario_RolValido_DeberiaPasarValidacion()
        {
            // Arrange
            var usuario = new Usuario
            {
                Email = "usuario@univalle.edu.bo",
                Rol = "DOCENTE"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(usuario);
            var isValid = Validator.TryValidateObject(usuario, validationContext, validationResults, true);

            // Assert
            var rolErrors = validationResults.Where(v => v.MemberNames.Contains("Rol"));
            Assert.Empty(rolErrors);
        }
    }
}

