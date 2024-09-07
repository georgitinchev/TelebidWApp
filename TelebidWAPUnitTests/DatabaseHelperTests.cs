using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using WebAppTelebid.Helpers;

namespace TelebidWAPUnitTests
{
    [TestClass]
    public class DatabaseHelperTests
    {
        private DatabaseHelper _databaseHelper;

        [TestInitialize]
        public void Setup()
        {
            _databaseHelper = new DatabaseHelper("your_connection_string");
        }

        [TestMethod]
        public void GetConnection_ShouldReturnSqlConnection()
        {
            var connection = _databaseHelper.GetConnection();

            Assert.IsNotNull(connection);
            Assert.IsInstanceOfType(connection, typeof(SqlConnection));
        }

        [TestMethod]
        public void OpenConnection_ShouldOpenSqlConnection()
        {
            var connection = new SqlConnection("your_connection_string");

            _databaseHelper.OpenConnection(connection);

            Assert.AreEqual(System.Data.ConnectionState.Open, connection.State);
        }

        [TestMethod]
        public void CloseConnection_ShouldCloseSqlConnection()
        {
            var connection = new SqlConnection("your_connection_string");
            _databaseHelper.OpenConnection(connection);

            _databaseHelper.CloseConnection(connection);

            Assert.AreEqual(System.Data.ConnectionState.Closed, connection.State);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OpenConnection_ShouldThrowArgumentNullException_WhenConnectionIsNull()
        {
            _databaseHelper.OpenConnection(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CloseConnection_ShouldThrowArgumentNullException_WhenConnectionIsNull()
        {
            _databaseHelper.CloseConnection(null);
        }
    }
}
