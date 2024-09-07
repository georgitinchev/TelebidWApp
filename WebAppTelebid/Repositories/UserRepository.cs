using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Models;

namespace WebAppTelebid.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseHelper _databaseHelper;

        public UserRepository(IDatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        public User GetUserById(int userId)
        {
            try
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    _databaseHelper.OpenConnection(connection);
                    using (SqlCommand cmd = new SqlCommand("SELECT id, email, name FROM users WHERE id = @UserId", connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    Id = (int)reader["id"],
                                    Email = (string)reader["email"],
                                    Name = (string)reader["name"]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user by ID: {ex.Message}");
            }
            return null;
        }

        public User GetUserByEmail(string email)
        {
            try
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    _databaseHelper.OpenConnection(connection);
                    using (SqlCommand cmd = new SqlCommand("SELECT id, email, name, password_hash, password_salt FROM users WHERE email = @Email", connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    Id = (int)reader["id"],
                                    Email = (string)reader["email"],
                                    Name = (string)reader["name"],
                                    PasswordHash = (string)reader["password_hash"],
                                    PasswordSalt = (byte[])reader["password_salt"]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user by email: {ex.Message}");
            }
            return null;
        }

        public bool RegisterUser(string email, string name, string password)
        {
            try
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    _databaseHelper.OpenConnection(connection);
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO users (email, name, password_hash, password_salt) VALUES (@Email, @Name, @PasswordHash, @PasswordSalt)", connection))
                    {
                        byte[] salt = GenerateSalt();
                        string hashedPassword = HashPassword(password, salt);

                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering user: {ex.Message}");
                return false;
            }
        }

        public bool UpdateUser(int userId, string email, string name, string? password = null)
        {
            try
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    _databaseHelper.OpenConnection(connection);

                    string query = "UPDATE users SET email = @Email, name = @Name";
                    if (!string.IsNullOrEmpty(password))
                    {
                        query += ", password_hash = @PasswordHash, password_salt = @PasswordSalt";
                    }
                    query += " WHERE id = @UserId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Name", name);

                        if (!string.IsNullOrEmpty(password))
                        {
                            byte[] salt = GenerateSalt();
                            string hashedPassword = HashPassword(password, salt);
                            command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                            command.Parameters.AddWithValue("@PasswordSalt", salt);
                        }

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return false;
            }
        }


        public bool CheckIfEmailExists(string email)
        {
            try
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    _databaseHelper.OpenConnection(connection);
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM users WHERE email = @Email", connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        return (int)cmd.ExecuteScalar() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if email exists: {ex.Message}");
                return false;
            }
        }

        private string HashPassword(string password, byte[] salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] saltedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
                return Convert.ToBase64String(sha256.ComputeHash(saltedPassword));
            }
        }

        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}
