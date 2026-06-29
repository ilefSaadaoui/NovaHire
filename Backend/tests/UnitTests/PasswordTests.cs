using Xunit;
using BCrypt.Net;

namespace UnitTests
{
    public class PasswordTests
    {
        [Fact]
        public void VerifyPassword_ShouldReturnTrue_ForValidPassword()
        {
            // Arrange
            string password = "SecurePassword123!";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            // Act
            bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);

            // Assert
            Assert.True(isValid, "La validation devrait réussir avec le bon mot de passe.");
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_ForInvalidPassword()
        {
            // Arrange
            string password = "SecurePassword123!";
            string wrongPassword = "WrongPassword!";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            // Act
            bool isValid = BCrypt.Net.BCrypt.Verify(wrongPassword, hash);

            // Assert
            Assert.False(isValid, "La validation devrait échouer avec un mauvais mot de passe.");
        }
    }
}
