using System;
using System.Data.SqlClient;
using WebAppTelebid.Interfaces;

public class DatabaseHelper : IDatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public void OpenConnection(SqlConnection connection)
    {
        try
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
        }
        catch (SqlException ex)
        {
            throw new Exception("Database connection error.", ex);
        }
    }

    public void CloseConnection(SqlConnection connection)
    {
        try
        {
            if (connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error closing connection: {ex.Message}");
        }
    }
}
