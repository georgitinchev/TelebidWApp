using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data.SqlClient;
using WebAppTelebid.Interfaces;

namespace UnitTests
{
    [TestClass]
    public class DatabaseHelperTests
    {
        private Mock<SqlConnection> _mockConnection;
        private IDatabaseHelper _databaseHelper;
        private string _connectionString = "connection-string";

        [TestInitialize]
        public void Setup()
        {
            _mockConnection = new Mock<SqlConnection>();
            _databaseHelper = new DatabaseHelper(_connectionString);
        }

        [TestMethod]
        public void GetConnection_ShouldReturnSqlConnection()
        {
            var result = _databaseHelper.GetConnection();
            Assert.IsNotNull(result);
            Assert.AreEqual(_connectionString, result.ConnectionString);
        }

        [TestMethod]
        public void OpenConnection_ShouldOpenConnection_WhenNotOpen()
        {
            _mockConnection.Setup(c => c.State).Returns(System.Data.ConnectionState.Closed);

            _databaseHelper.OpenConnection(_mockConnection.Object);

            _mockConnection.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        public void OpenConnection_ShouldNotOpenConnection_WhenAlreadyOpen()
        {
            _mockConnection.Setup(c => c.State).Returns(System.Data.ConnectionState.Open);

            _databaseHelper.OpenConnection(_mockConnection.Object);

            _mockConnection.Verify(c => c.Open(), Times.Never);
        }

        [TestMethod]
        public void CloseConnection_ShouldCloseConnection_WhenNotClosed()
        {
            _mockConnection.Setup(c => c.State).Returns(System.Data.ConnectionState.Open);

            _databaseHelper.CloseConnection(_mockConnection.Object);

            _mockConnection.Verify(c => c.Close(), Times.Once);
        }

        [TestMethod]
        public void CloseConnection_ShouldNotCloseConnection_WhenAlreadyClosed()
        {
            _mockConnection.Setup(c => c.State).Returns(System.Data.ConnectionState.Closed);

            _databaseHelper.CloseConnection(_mockConnection.Object);

            _mockConnection.Verify(c => c.Close(), Times.Never);
        }
    }
}
