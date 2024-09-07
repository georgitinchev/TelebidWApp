using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data.SqlClient;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Models;
using WebAppTelebid.Repositories;

namespace TelebidWAPUnitTests
{
    [TestClass]
    public class UserRepositoryTests
    {
        private Mock<IDatabaseHelper> _mockDatabaseHelper;
        private UserRepository _userRepository;

        [TestInitialize]
        public void Setup()
        {
            _mockDatabaseHelper = new Mock<IDatabaseHelper>();
            _userRepository = new UserRepository(_mockDatabaseHelper.Object);
        }

        [TestMethod]
        public void GetUserById_ShouldReturnUser_WhenUserExists()
        {
            var userId = 1;
            var user = new User { Id = userId, Email = "test@example.com", Name = "Test User" };
            var mockConnection = new Mock<SqlConnection>();
            var mockCommand = new Mock<SqlCommand>();
            var mockReader = new Mock<SqlDataReader>();

            _mockDatabaseHelper.Setup(d => d.GetConnection()).Returns(mockConnection.Object);
            _mockDatabaseHelper.Setup(d => d.OpenConnection(mockConnection.Object));
            mockCommand.Setup(c => c.ExecuteReader()).Returns(mockReader.Object);
            mockReader.Setup(r => r.Read()).Returns(true);
            mockReader.Setup(r => r["id"]).Returns(user.Id);
            mockReader.Setup(r => r["email"]).Returns(user.Email);
            mockReader.Setup(r => r["name"]).Returns(user.Name);

            var result = _userRepository.GetUserById(userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(user.Email, result.Email);
            Assert.AreEqual(user.Name, result.Name);
        }

        [TestMethod]
        public void GetUserByEmail_ShouldReturnUser_WhenUserExists()
        {
            var email = "test@example.com";
            var user = new User { Id = 1, Email = email, Name = "Test User", PasswordHash = "hashedPassword", PasswordSalt = new byte[16] };
            var mockConnection = new Mock<SqlConnection>();
            var mockCommand = new Mock<SqlCommand>();
            var mockReader = new Mock<SqlDataReader>();

            _mockDatabaseHelper.Setup(d => d.GetConnection()).Returns(mockConnection.Object);
            _mockDatabaseHelper.Setup(d => d.OpenConnection(mockConnection.Object));
            mockCommand.Setup(c => c.ExecuteReader()).Returns(mockReader.Object);
            mockReader.Setup(r => r.Read()).Returns(true);
            mockReader.Setup(r => r["id"]).Returns(user.Id);
            mockReader.Setup(r => r["email"]).Returns(user.Email);
            mockReader.Setup(r => r["name"]).Returns(user.Name);
            mockReader.Setup(r => r["password_hash"]).Returns(user.PasswordHash);
            mockReader.Setup(r => r["password_salt"]).Returns(user.PasswordSalt);

            var result = _userRepository.GetUserByEmail(email);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(user.Email, result.Email);
            Assert.AreEqual(user.Name, result.Name);
            Assert.AreEqual(user.PasswordHash, result.PasswordHash);
            Assert.AreEqual(user.PasswordSalt, result.PasswordSalt);
        }

        [TestMethod]
        public void RegisterUser_ShouldReturnTrue_WhenUserIsRegistered()
        {
            var email = "test@example.com";
            var name = "Test User";
            var password = "password";
            var mockConnection = new Mock<SqlConnection>();
            var mockCommand = new Mock<SqlCommand>();

            _mockDatabaseHelper.Setup(d => d.GetConnection()).Returns(mockConnection.Object);
            _mockDatabaseHelper.Setup(d => d.OpenConnection(mockConnection.Object));
            mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            var result = _userRepository.RegisterUser(email, name, password);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UpdateUser_ShouldReturnTrue_WhenUserIsUpdated()
        {
            var userId = 1;
            var email = "updated@example.com";
            var name = "Updated User";
            var password = "newpassword";
            var mockConnection = new Mock<SqlConnection>();
            var mockCommand = new Mock<SqlCommand>();

            _mockDatabaseHelper.Setup(d => d.GetConnection()).Returns(mockConnection.Object);
            _mockDatabaseHelper.Setup(d => d.OpenConnection(mockConnection.Object));
            mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            var result = _userRepository.UpdateUser(userId, email, name, password);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CheckIfEmailExists_ShouldReturnTrue_WhenEmailExists()
        {
            var email = "test@example.com";
            var mockConnection = new Mock<SqlConnection>();
            var mockCommand = new Mock<SqlCommand>();

            _mockDatabaseHelper.Setup(d => d.GetConnection()).Returns(mockConnection.Object);
            _mockDatabaseHelper.Setup(d => d.OpenConnection(mockConnection.Object));
            mockCommand.Setup(c => c.ExecuteScalar()).Returns(1);

            var result = _userRepository.CheckIfEmailExists(email);

            Assert.IsTrue(result);
        }
    }
}
