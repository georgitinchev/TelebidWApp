using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data.SqlClient;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Models;
using WebAppTelebid.Repositories;

namespace UnitTests
{
    [TestClass]
    public class UserRepositoryTests
    {
        private Mock<IDatabaseHelper> _mockDatabaseHelper;
        private UserRepository _userRepository;
        private Mock<SqlConnection> _mockConnection;
        private Mock<SqlCommand> _mockCommand;
        private Mock<SqlDataReader> _mockReader;

        [TestInitialize]
        public void Setup()
        {
            _mockDatabaseHelper = new Mock<IDatabaseHelper>();
            _mockConnection = new Mock<SqlConnection>();
            _mockCommand = new Mock<SqlCommand>();
            _mockReader = new Mock<SqlDataReader>();
            _userRepository = new UserRepository(_mockDatabaseHelper.Object);
        }

        [TestMethod]
        public void GetUserById_ShouldReturnUser_WhenUserExists()
        {
            int userId = 1;
            var expectedUser = new User { Id = userId, Email = "test@example.com", Name = "Test User" };

            SetupMockSqlConnection();
            _mockReader.Setup(r => r.Read()).Returns(true);
            _mockReader.Setup(r => r["id"]).Returns(expectedUser.Id);
            _mockReader.Setup(r => r["email"]).Returns(expectedUser.Email);
            _mockReader.Setup(r => r["name"]).Returns(expectedUser.Name);

            var result = _userRepository.GetUserById(userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUser.Id, result.Id);
            Assert.AreEqual(expectedUser.Email, result.Email);
            Assert.AreEqual(expectedUser.Name, result.Name);
        }

        [TestMethod]
        public void GetUserById_ShouldReturnNull_WhenUserDoesNotExist()
        {
            SetupMockSqlConnection();
            _mockReader.Setup(r => r.Read()).Returns(false);

            var result = _userRepository.GetUserById(999);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetUserByEmail_ShouldReturnUser_WhenUserExists()
        {
            string email = "test@example.com";
            var expectedUser = new User { Id = 1, Email = email, Name = "Test User" };

            SetupMockSqlConnection();
            _mockReader.Setup(r => r.Read()).Returns(true);
            _mockReader.Setup(r => r["id"]).Returns(expectedUser.Id);
            _mockReader.Setup(r => r["email"]).Returns(expectedUser.Email);
            _mockReader.Setup(r => r["name"]).Returns(expectedUser.Name);

            var result = _userRepository.GetUserByEmail(email);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUser.Id, result.Id);
            Assert.AreEqual(expectedUser.Email, result.Email);
            Assert.AreEqual(expectedUser.Name, result.Name);
        }

        [TestMethod]
        public void GetUserByEmail_ShouldReturnNull_WhenUserDoesNotExist()
        {
            SetupMockSqlConnection();
            _mockReader.Setup(r => r.Read()).Returns(false);

            var result = _userRepository.GetUserByEmail("nonexistent@example.com");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RegisterUser_ShouldReturnTrue_WhenRegistrationIsSuccessful()
        {
            SetupMockSqlConnectionForNonQuery(1);

            var result = _userRepository.RegisterUser("test@example.com", "Test User", "password");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RegisterUser_ShouldReturnFalse_WhenRegistrationFails()
        {
            SetupMockSqlConnectionForNonQuery(0);

            var result = _userRepository.RegisterUser("test@example.com", "Test User", "password");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UpdateUser_ShouldReturnTrue_WhenUpdateIsSuccessful()
        {
            SetupMockSqlConnectionForNonQuery(1);

            var result = _userRepository.UpdateUser(1, "updated@example.com", "Updated User", "newpassword");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UpdateUser_ShouldReturnFalse_WhenUpdateFails()
        {
            SetupMockSqlConnectionForNonQuery(0);

            var result = _userRepository.UpdateUser(1, "updated@example.com", "Updated User", "newpassword");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CheckIfEmailExists_ShouldReturnTrue_WhenEmailExists()
        {
            SetupMockSqlConnectionForScalar(1);

            var result = _userRepository.CheckIfEmailExists("test@example.com");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CheckIfEmailExists_ShouldReturnFalse_WhenEmailDoesNotExist()
        {
            SetupMockSqlConnectionForScalar(0);

            var result = _userRepository.CheckIfEmailExists("nonexistent@example.com");

            Assert.IsFalse(result);
        }

        private void SetupMockSqlConnection()
        {
            _mockDatabaseHelper.Setup(d => d.GetConnection()).Returns(_mockConnection.Object);
            _mockDatabaseHelper.Setup(d => d.OpenConnection(It.IsAny<SqlConnection>()));
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);
        }

        private void SetupMockSqlConnectionForNonQuery(int result)
        {
            _mockDatabaseHelper.Setup(d => d.GetConnection()).Returns(_mockConnection.Object);
            _mockDatabaseHelper.Setup(d => d.OpenConnection(It.IsAny<SqlConnection>()));
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(result);
        }

        private void SetupMockSqlConnectionForScalar(object result)
        {
            _mockDatabaseHelper.Setup(d => d.GetConnection()).Returns(_mockConnection.Object);
            _mockDatabaseHelper.Setup(d => d.OpenConnection(It.IsAny<SqlConnection>()));
            _mockCommand.Setup(c => c.ExecuteScalar()).Returns(result);
        }
    }
}
