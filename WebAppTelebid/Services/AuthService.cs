using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Models;

namespace WebAppTelebid.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public int AuthenticateUser(string email, string password)
        {
            try
            {
                User user = _userRepository.GetUserByEmail(email);
                if (user != null)
                {
                    string hashedPassword = HashPassword(password, user.PasswordSalt);
                    if (hashedPassword == user.PasswordHash)
                    {
                        return user.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during authentication: {ex.Message}");
            }
            return -1;
        }

        private string HashPassword(string password, byte[] salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] saltedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
                return Convert.ToBase64String(sha256.ComputeHash(saltedPassword));
            }
        }
    }
}
