using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using WebAppTelebid.Interfaces;

namespace TestProject
{
    [TestClass]
    public class DatabaseHelperTests
    {
        private IDatabaseHelper _databaseHelper;
        private string _connectionString = "Server=Georgi-PC\\SQLEXPRESS;Database=WebAppTelebid;User Id=telebidPro;Password=e9999619;"; 

        [TestInitialize]
        public void SetUp()
        {
            _databaseHelper = new DatabaseHelper(_connectionString);
        }

        [TestMethod]
        public void GetConnection_ShouldReturnSqlConnection()
        {
            SqlConnection connection = _databaseHelper.GetConnection();
            Assert.IsNotNull(connection);
            Assert.AreEqual(_connectionString, connection.ConnectionString);
        }

        [TestMethod]
        public void OpenConnection_ShouldOpenSqlConnection()
        {
            SqlConnection connection = _databaseHelper.GetConnection();
            _databaseHelper.OpenConnection(connection);

            Assert.AreEqual(System.Data.ConnectionState.Open, connection.State);
        }

        [TestMethod]
        public void CloseConnection_ShouldCloseSqlConnection()
        {
            SqlConnection connection = _databaseHelper.GetConnection();
            _databaseHelper.OpenConnection(connection);
            _databaseHelper.CloseConnection(connection);

            Assert.AreEqual(System.Data.ConnectionState.Closed, connection.State);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Database connection error.")]
        public void OpenConnection_ShouldThrowException_WhenConnectionFails()
        {
            string invalidConnectionString = "Data Source=invalid;Initial Catalog=TestDb;Integrated Security=True";
            IDatabaseHelper invalidDatabaseHelper = new DatabaseHelper(invalidConnectionString);
            SqlConnection connection = invalidDatabaseHelper.GetConnection();
            invalidDatabaseHelper.OpenConnection(connection);
        }

        [TestMethod]
        public void CloseConnection_ShouldDoNothing_WhenConnectionIsAlreadyClosed()
        {
            SqlConnection connection = _databaseHelper.GetConnection();
            _databaseHelper.CloseConnection(connection);

            Assert.AreEqual(System.Data.ConnectionState.Closed, connection.State);
        }
    }
}
