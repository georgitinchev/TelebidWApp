using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Text;
using WebAppTelebid;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Models;

namespace UnitTests
{
    [TestClass]
    public class RequestHandlerTests
    {
        private Mock<IAuthService> _mockAuthService = null!;
        private Mock<IUserRepository> _mockUserRepository = null!;
        private Mock<IRequestHandler> _mockRequestHandler = null!;
        private Mock<IHttpListenerContext> _mockContext = null!;
        private Mock<HttpListenerRequest> _mockRequest = null!;
        private Mock<HttpListenerResponse> _mockResponse = null!;
        private Mock<ISqlConnection> _mockSqlConnection = null!;
        private MemoryStream _outputStream = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRequestHandler = new Mock<IRequestHandler>();

            // Mock IHttpListenerContext
            _mockContext = new Mock<IHttpListenerContext>();
            _mockRequest = new Mock<HttpListenerRequest>();
            _mockResponse = new Mock<HttpListenerResponse>();
            _mockContext.Setup(c => c.Request).Returns(_mockRequest.Object);
            _mockContext.Setup(c => c.Response).Returns(_mockResponse.Object);

            // Mock ISqlConnection
            _mockSqlConnection = new Mock<ISqlConnection>();
            _mockSqlConnection.Setup(conn => conn.Open());
            _mockSqlConnection.Setup(conn => conn.Close());
            _mockSqlConnection.Setup(conn => conn.State).Returns(System.Data.ConnectionState.Open);

            _outputStream = new MemoryStream();
            _mockResponse.Setup(r => r.OutputStream).Returns(_outputStream);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _outputStream.Dispose();
        }

        [TestMethod]
        public void HandleRequest_ShouldServeLoginPage_OnRootRoute()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/"));
            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);

            _mockResponse.Verify(r => r.Redirect(It.IsAny<string>()), Times.Never);
            Assert.IsTrue(_outputStream.Length > 0);
        }

        [TestMethod]
        public void HandleRequest_ShouldServe404_OnUnknownRoute()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/unknown"));
            _mockResponse.SetupProperty(r => r.StatusCode);

            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);

            Assert.AreEqual(404, _mockResponse.Object.StatusCode);
            Assert.IsTrue(_outputStream.Length > 0);
        }

        [TestMethod]
        public void HandleRegister_ShouldReturnError_WhenCaptchaIsInvalid()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/register"));
            _mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            _mockRequest.Setup(r => r.Cookies).Returns(new CookieCollection { new Cookie("captchaCode", "12345") });
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("email=test@example.com&name=Test&password=password&captcha=54321"));
            _mockRequest.Setup(r => r.InputStream).Returns(inputStream);

            _mockResponse.SetupProperty(r => r.StatusCode);
            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);

            Assert.AreEqual(400, _mockResponse.Object.StatusCode);
            Assert.IsTrue(_outputStream.Length > 0);
        }

        [TestMethod]
        public void HandleLogin_ShouldAuthenticateUser_WhenCredentialsAreCorrect()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/login"));
            _mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("email=test@example.com&password=password&captcha=12345"));
            _mockRequest.Setup(r => r.InputStream).Returns(inputStream);
            _mockRequest.Setup(r => r.Cookies).Returns(new CookieCollection { new Cookie("captchaCode", "12345") });
            _mockAuthService.Setup(s => s.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>())).Returns(1);
            _mockUserRepository.Setup(r => r.GetUserById(It.IsAny<int>())).Returns(new User { Id = 1, Name = "Test User" });

            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);

            _mockResponse.Verify(r => r.Redirect(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void HandleRegister_ShouldRegisterUser_WhenValidDataProvided()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/register"));
            _mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            _mockRequest.Setup(r => r.Cookies).Returns(new CookieCollection { new Cookie("captchaCode", "12345") });
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("email=test@example.com&name=Test&password=password&captcha=12345"));
            _mockRequest.Setup(r => r.InputStream).Returns(inputStream);
            _mockUserRepository.Setup(r => r.CheckIfEmailExists(It.IsAny<string>())).Returns(false);
            _mockUserRepository.Setup(r => r.RegisterUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockUserRepository.Setup(r => r.GetUserByEmail(It.IsAny<string>())).Returns(new User { Id = 1, Name = "Test User" });

            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);

            _mockResponse.Verify(r => r.Redirect(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void HandleRegister_ShouldReturnError_WhenEmailAlreadyExists()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/register"));
            _mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            _mockRequest.Setup(r => r.Cookies).Returns(new CookieCollection { new Cookie("captchaCode", "12345") });
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("email=test@example.com&name=Test&password=password&captcha=12345"));
            _mockRequest.Setup(r => r.InputStream).Returns(inputStream);
            _mockUserRepository.Setup(r => r.CheckIfEmailExists(It.IsAny<string>())).Returns(true);
            _mockResponse.SetupProperty(r => r.StatusCode);
            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);
            Assert.AreEqual(400, _mockResponse.Object.StatusCode);
        }

        [TestMethod]
        public void HandleLogin_ShouldReturnError_WhenCaptchaIsInvalid()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/login"));
            _mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            _mockRequest.Setup(r => r.Cookies).Returns(new CookieCollection { new Cookie("captchaCode", "12345") });
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("email=test@example.com&password=password&captcha=54321"));
            _mockRequest.Setup(r => r.InputStream).Returns(inputStream);
            _mockResponse.SetupProperty(r => r.StatusCode);
            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);
            Assert.AreEqual(400, _mockResponse.Object.StatusCode);
        }

        [TestMethod]
        public void HandleLogin_ShouldReturnError_WhenCredentialsAreInvalid()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/login"));
            _mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            _mockRequest.Setup(r => r.Cookies).Returns(new CookieCollection { new Cookie("captchaCode", "12345") });
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("email=test@example.com&password=wrongpassword&captcha=12345"));
            _mockRequest.Setup(r => r.InputStream).Returns(inputStream);
            _mockAuthService.Setup(a => a.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>())).Returns(-1);
            _mockResponse.SetupProperty(r => r.StatusCode);
            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);
            Assert.AreEqual(400, _mockResponse.Object.StatusCode);
        }

        [TestMethod]
        public void HandleUpdate_ShouldUpdateProfile_WhenValidDataProvided()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/update"));
            _mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("userId=1&email=updated@example.com&name=Updated&password=password"));
            _mockRequest.Setup(r => r.InputStream).Returns(inputStream);
            _mockUserRepository.Setup(r => r.UpdateUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockUserRepository.Setup(r => r.GetUserById(It.IsAny<int>())).Returns(new User { Id = 1, Name = "Updated User" });
            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);
            _mockResponse.Verify(r => r.Redirect(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void HandleUpdate_ShouldReturnError_WhenEmailIsAlreadyTaken()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/update"));
            _mockRequest.Setup(r => r.HttpMethod).Returns("POST");
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("userId=1&email=taken@example.com&name=Updated"));
            _mockRequest.Setup(r => r.InputStream).Returns(inputStream);
            _mockUserRepository.Setup(r => r.GetUserByEmail(It.IsAny<string>())).Returns(new User { Id = 2, Email = "taken@example.com" });
            _mockResponse.SetupProperty(r => r.StatusCode);
            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);
            Assert.AreEqual(400, _mockResponse.Object.StatusCode);
        }

        [TestMethod]
        public void HandleLogout_ShouldClearUserCookie()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/logout"));
            _mockRequest.Setup(r => r.Cookies).Returns(new CookieCollection { new Cookie("userId", "1") });
            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);
            _mockResponse.Verify(r => r.Cookies.Add(It.Is<Cookie>(c => c.Expires < DateTime.Now)), Times.Once);
        }

        [TestMethod]
        public void HandleDashboard_ShouldReturnDashboard_WhenUserIsAuthenticated()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/dashboard"));
            _mockRequest.Setup(r => r.Cookies).Returns(new CookieCollection { new Cookie("userId", "1") });
            _mockUserRepository.Setup(r => r.GetUserById(It.IsAny<int>())).Returns(new User { Id = 1, Name = "Test User" });
            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);
            Assert.IsTrue(_outputStream.Length > 0);
        }

        [TestMethod]
        public void HandleDashboard_ShouldRedirectToLogin_WhenUserIsNotAuthenticated()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/dashboard"));
            _mockRequest.Setup(r => r.Cookies).Returns(new CookieCollection());
            _mockRequestHandler.Object.HandleRequest(_mockContext.Object);
            _mockResponse.Verify(r => r.Redirect("/login"), Times.Once);
        }

        [TestMethod]
        public void HandleCaptcha_ShouldGenerateCaptchaImage()
        {
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/captcha"));
            _mockResponse.Setup(r => r.ContentType).Returns("image/png");
            _mockRequestHandler.Object.HandleCaptcha(_mockContext.Object);
            Assert.AreEqual("image/png", _mockResponse.Object.ContentType);
            Assert.IsTrue(_outputStream.Length > 0);
        }
    }
}
