using Moq;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Models;
using WebAppTelebid.Services;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class AuthServiceTests
{
    private Mock<IUserRepository> _mockUserRepository;
    private AuthService _authService;

    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _authService = new AuthService(_mockUserRepository.Object);
    }

    [TestMethod]
    public void AuthenticateUser_ShouldReturnUserId_WhenCredentialsAreCorrect()
    {
        string email = "test@example.com";
        string password = "password";
        byte[] salt = new byte[16];
        string hashedPassword = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password).Concat(salt).ToArray()));

        var user = new User { Id = 1, Email = email, PasswordSalt = salt, PasswordHash = hashedPassword };
        _mockUserRepository.Setup(r => r.GetUserByEmail(email)).Returns(user);

        var result = _authService.AuthenticateUser(email, password);

        Assert.AreEqual(user.Id, result);
    }

    [TestMethod]
    public void AuthenticateUser_ShouldReturnMinusOne_WhenCredentialsAreIncorrect()
    {
        string email = "test@example.com";
        string password = "wrongpassword";
        byte[] salt = new byte[16];
        string hashedPassword = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("password").Concat(salt).ToArray()));

        var user = new User { Id = 1, Email = email, PasswordSalt = salt, PasswordHash = hashedPassword };
        _mockUserRepository.Setup(r => r.GetUserByEmail(email)).Returns(user);

        var result = _authService.AuthenticateUser(email, password);

        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void AuthenticateUser_ShouldReturnMinusOne_WhenUserDoesNotExist()
    {
        string email = "nonexistent@example.com";
        string password = "password";
        _mockUserRepository.Setup(r => r.GetUserByEmail(email)).Returns((User)null);

        var result = _authService.AuthenticateUser(email, password);

        Assert.AreEqual(-1, result);
    }
}
