using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Models;
using WebAppTelebid.Repositories;

namespace TestProject
{
    [TestClass]
    public class UserRepositoryTests
    {
        private SqliteConnection _connection = null!;
        private IUserRepository _userRepository = null!;

        [TestInitialize]
        public void SetUp()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var command = _connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE users (
                    id INTEGER PRIMARY KEY,
                    email TEXT NOT NULL,
                    name TEXT NOT NULL,
                    password_hash TEXT,
                    password_salt BLOB
                );
            ";
            command.ExecuteNonQuery();

            _userRepository = new UserRepository(new DatabaseHelper(_connection.ConnectionString));
        }

        [TestCleanup]
        public void TearDown()
        {
            _connection.Close();
        }

        [TestMethod]
        public void GetUserById_ShouldReturnUser_WhenUserExists()
        {
            var insertCommand = _connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO users (email, name)
                VALUES ('test@example.com', 'Test User');
            ";
            insertCommand.ExecuteNonQuery();

            User result = _userRepository.GetUserById(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("test@example.com", result.Email);
        }

        [TestMethod]
        public void GetUserById_ShouldReturnNull_WhenUserDoesNotExist()
        {
            User result = _userRepository.GetUserById(1000);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RegisterUser_ShouldReturnTrue_WhenUserIsRegisteredSuccessfully()
        {
            bool result = _userRepository.RegisterUser("test@example.com", "Test", "password");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RegisterUser_ShouldReturnFalse_WhenRegistrationFails()
        {
            _connection.Close();
            bool result = _userRepository.RegisterUser("test@example.com", "Test", "password");

            Assert.IsFalse(result);
        }
    }
}
