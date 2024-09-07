using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Security.Cryptography;
using System.Text;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Models;
using WebAppTelebid.Services;

namespace TelebidWAPUnitTests
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private AuthService _authService;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _authService = new AuthService(_mockUserRepository.Object);
        }

        [TestMethod]
        public void AuthenticateUser_ShouldReturnUserId_WhenCredentialsAreValid()
        {
            var email = "test@example.com";
            var password = "password";
            var user = new User
            {
                Id = 1,
                Email = email,
                PasswordHash = HashPassword(password, out byte[] salt),
                PasswordSalt = salt
            };

            _mockUserRepository.Setup(r => r.GetUserByEmail(email)).Returns(user);

            var result = _authService.AuthenticateUser(email, password);

            Assert.AreEqual(user.Id, result);
        }

        [TestMethod]
        public void AuthenticateUser_ShouldReturnMinusOne_WhenCredentialsAreInvalid()
        {
            var email = "test@example.com";
            var password = "wrongpassword";
            var user = new User
            {
                Id = 1,
                Email = email,
                PasswordHash = HashPassword("password", out byte[] salt),
                PasswordSalt = salt
            };

            _mockUserRepository.Setup(r => r.GetUserByEmail(email)).Returns(user);

            var result = _authService.AuthenticateUser(email, password);

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void AuthenticateUser_ShouldReturnMinusOne_WhenUserDoesNotExist()
        {
            var email = "nonexistent@example.com";
            var password = "password";

            _mockUserRepository.Setup(r => r.GetUserByEmail(email)).Returns((User)null);

            var result = _authService.AuthenticateUser(email, password);

            Assert.AreEqual(-1, result);
        }

        private string HashPassword(string password, out byte[] salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                salt = new byte[16];
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(salt);
                }
                byte[] saltedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
                return Convert.ToBase64String(sha256.ComputeHash(saltedPassword));
            }
        }
    }
}
