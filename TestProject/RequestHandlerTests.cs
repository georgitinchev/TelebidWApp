using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using WebAppTelebid;
using Moq;
using WebAppTelebid.Interfaces;

namespace TestProject
{
    [TestClass]
    public class RequestHandlerTests
    {
        private RequestHandler _requestHandler;
        private HttpListener _listener;

        [TestInitialize]
        public void SetUp()
        {
            _requestHandler = new RequestHandler(new Mock<IAuthService>().Object, new Mock<IUserRepository>().Object);
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:8080/");
            _listener.Start();
        }

        [TestCleanup]
        public void CleanUp()
        {
            _listener.Stop();
        }

        [TestMethod]
        public async Task HandleRequest_ShouldServeLoginPage_WhenPathIsRoot()
        {
            var requestTask = CreateHttpRequest("/");

            HttpListenerContext context = await _listener.GetContextAsync();
            _requestHandler.HandleRequest(context);

            Assert.AreEqual(200, context.Response.StatusCode);
            await requestTask;
        }

        [TestMethod]
        public async Task HandleRegister_ShouldRegisterUser_WhenDataIsValid()
        {
            var requestTask = CreateHttpPostRequest("/register", "email=test@example.com&name=Test&password=password&captcha=captcha");

            HttpListenerContext context = await _listener.GetContextAsync();
            _requestHandler.HandleRequest(context);

            Assert.AreEqual(200, context.Response.StatusCode);
            await requestTask;
        }

        [TestMethod]
        public async Task HandleRegister_ShouldFail_WhenEmailAlreadyExists()
        {
            var requestTask = CreateHttpPostRequest("/register", "email=test@example.com&name=Test&password=password&captcha=captcha");

            HttpListenerContext context = await _listener.GetContextAsync();
            _requestHandler.HandleRegister(context);

            Assert.AreEqual(200, context.Response.StatusCode);
            await requestTask;
        }

        [TestMethod]
        public async Task HandleLogin_ShouldServeDashboard_WhenLoginIsSuccessful()
        {
            var requestTask = CreateHttpPostRequest("/login", "email=test@example.com&password=password&captcha=captcha");

            HttpListenerContext context = await _listener.GetContextAsync();
            _requestHandler.HandleLogin(context);

            Assert.AreEqual(200, context.Response.StatusCode);
            await requestTask;
        }

        [TestMethod]
        public async Task HandleDashboard_ShouldServeDashboard_WhenUserIsAuthenticated()
        {
            var requestTask = CreateHttpRequestWithCookie("/dashboard", "userId=1");

            HttpListenerContext context = await _listener.GetContextAsync();
            _requestHandler.HandleDashboard(context);

            Assert.AreEqual(200, context.Response.StatusCode);
            await requestTask;
        }

        private async Task CreateHttpRequest(string path)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://localhost:8080{path}");
            request.Method = "GET";
            await request.GetResponseAsync();
        }

        private async Task CreateHttpPostRequest(string path, string postData)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://localhost:8080{path}");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(postData);
            request.ContentLength = dataBytes.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(dataBytes, 0, dataBytes.Length);
            }

            await request.GetResponseAsync();
        }

        private async Task CreateHttpRequestWithCookie(string path, string cookieValue)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://localhost:8080{path}");
            request.Method = "GET";
            request.Headers.Add("Cookie", cookieValue);
            await request.GetResponseAsync();
        }
    }
}
