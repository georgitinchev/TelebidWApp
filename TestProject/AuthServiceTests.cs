using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Models;
using WebAppTelebid.Services;

namespace TestProject
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private IAuthService _authService;

        [TestInitialize]
        public void SetUp()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authService = new AuthService(_userRepositoryMock.Object);
        }

        [TestMethod]
        public void AuthenticateUser_ShouldReturnUserId_WhenCredentialsAreValid()
        {
            string email = "test@example.com";
            string password = "password";
            byte[] salt = new byte[] { 1, 2, 3, 4, 5 };
            User user = new User { Id = 1, Email = email, PasswordSalt = salt, PasswordHash = _authService.HashPassword(password, salt) };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(email)).Returns(user);

            int result = _authService.AuthenticateUser(email, password);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void AuthenticateUser_ShouldReturnMinusOne_WhenCredentialsAreInvalid()
        {
            string email = "test@example.com";
            string password = "wrongPassword";
            byte[] salt = new byte[] { 1, 2, 3, 4, 5 };
            User user = new User { Id = 1, Email = email, PasswordSalt = salt, PasswordHash = _authService.HashPassword("password", salt) };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(email)).Returns(user);

            int result = _authService.AuthenticateUser(email, password);

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void AuthenticateUser_ShouldReturnMinusOne_WhenUserDoesNotExist()
        {
            string email = "nonexistent@example.com";
            string password = "password";

            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(email)).Returns((User)null);

            int result = _authService.AuthenticateUser(email, password);

            Assert.AreEqual(-1, result);
        }
    }
}
