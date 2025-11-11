using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace GestionLlaves.Tests.Helpers
{
    public class PasswordHelperTests
    {
        private byte[] HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        [Fact]
        public void HashPassword_ConMismaPassword_DeberiaGenerarMismoHash()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hash1 = HashPassword(password);
            var hash2 = HashPassword(password);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void HashPassword_ConDiferentesPasswords_DeberiaGenerarDiferentesHashes()
        {
            // Arrange
            var password1 = "Password1";
            var password2 = "Password2";

            // Act
            var hash1 = HashPassword(password1);
            var hash2 = HashPassword(password2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void HashPassword_ConPasswordVacia_DeberiaGenerarHash()
        {
            // Arrange
            var password = "";

            // Act
            var hash = HashPassword(password);

            // Assert
            Assert.NotNull(hash);
            Assert.Equal(32, hash.Length); // SHA256 produce 32 bytes
        }

        [Fact]
        public void HashPassword_ConPasswordLarga_DeberiaGenerarHash()
        {
            // Arrange
            var password = new string('A', 1000);

            // Act
            var hash = HashPassword(password);

            // Assert
            Assert.NotNull(hash);
            Assert.Equal(32, hash.Length);
        }
    }
}

