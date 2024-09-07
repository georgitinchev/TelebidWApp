using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Text;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Models;
using WebAppTelebid.Handlers;

namespace TelebidWAPUnitTests
{
    [TestClass]
    public class RequestHandlerTests
    {
        private Mock<IAuthService> _mockAuthService;
        private Mock<IUserRepository> _mockUserRepository;
        private RequestHandler _requestHandler;

        [TestInitialize]
        public void Setup()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockUserRepository = new Mock<IUserRepository>();
            _requestHandler = new RequestHandler(_mockAuthService.Object, _mockUserRepository.Object);
        }

        [TestMethod]
        public void HandleRequest_ShouldServeDefaultRoute_WhenPathIsRoot()
        {
            var context = new Mock<HttpListenerContext>();
            var request = new Mock<HttpListenerRequest>();
            var response = new Mock<HttpListenerResponse>();
            var outputStream = new MemoryStream();

            request.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/"));
            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response).Returns(response.Object);
            response.Setup(r => r.OutputStream).Returns(outputStream);

            _requestHandler.HandleRequest(context.Object);

            var responseString = Encoding.UTF8.GetString(outputStream.ToArray());
            Assert.IsTrue(responseString.Contains("Welcome to the default route"));
        }

        [TestMethod]
        public void HandleRequest_ShouldAuthenticateUser_WhenPathIsLogin()
        {
            var context = new Mock<HttpListenerContext>();
            var request = new Mock<HttpListenerRequest>();
            var response = new Mock<HttpListenerResponse>();
            var outputStream = new MemoryStream();
            var email = "test@example.com";
            var password = "password";
            var userId = 1;

            request.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/login"));
            request.Setup(r => r.HttpMethod).Returns("POST");
            request.Setup(r => r.InputStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes($"email={email}&password={password}")));
            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response).Returns(response.Object);
            response.Setup(r => r.OutputStream).Returns(outputStream);

            _mockAuthService.Setup(a => a.AuthenticateUser(email, password)).Returns(userId);

            _requestHandler.HandleRequest(context.Object);

            var responseString = Encoding.UTF8.GetString(outputStream.ToArray());
            Assert.IsTrue(responseString.Contains("Authentication successful"));
        }

        [TestMethod]
        public void HandleRequest_ShouldReturnNotFound_WhenPathIsUnknown()
        {
            var context = new Mock<HttpListenerContext>();
            var request = new Mock<HttpListenerRequest>();
            var response = new Mock<HttpListenerResponse>();
            var outputStream = new MemoryStream();

            request.Setup(r => r.Url).Returns(new Uri("http://localhost:8080/unknown"));
            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response).Returns(response.Object);
            response.Setup(r => r.OutputStream).Returns(outputStream);

            _requestHandler.HandleRequest(context.Object);

            var responseString = Encoding.UTF8.GetString(outputStream.ToArray());
            Assert.IsTrue(responseString.Contains("404 Not Found"));
        }
    }
}

