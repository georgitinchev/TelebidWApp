using System.Drawing;
using System.Net;
using System.Text;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Models;
using System.Drawing.Imaging;

namespace WebAppTelebid
{
    public class RequestHandler : IRequestHandler
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;

        public RequestHandler(IAuthService authService, IUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        public void HandleRequest(HttpListenerContext context) 
        {
            string path = context.Request.Url?.AbsolutePath.ToLower() ?? "/";
            Console.WriteLine($"Handling request for path: {path}");

            if (path == "/")
            {
                HandleDefaultRoute(context);
                return;
            }

            switch (path)
            {
                case "/register":
                    HandleRegister(context);
                    break;
                case "/login":
                    HandleLogin(context);
                    break;
                case "/update":
                    HandleUpdate(context);
                    break;
                case "/logout":
                    HandleLogout(context);
                    break;
                case "/dashboard":
                    HandleDashboard(context);
                    break;
                case "/captcha":
                    HandleCaptcha(context);
                    break;
                default:
                    Serve404(context);
                    break;
            }
        }

        private void HandleDefaultRoute(HttpListenerContext context)
        {
            if (context.Request.Cookies["userId"]?.Value != null)
            {
                context.Response.Redirect("/dashboard");
            }
            else
            {
                ServeHtml(context, "login.html");
            }
        }

        public void HandleRegister(HttpListenerContext context)
        {
            try
            {
                if (context.Request.HttpMethod == "POST")
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        string formData = reader.ReadToEnd();
                        var parsedData = ParseFormData(formData);
                        string email = parsedData["email"];
                        string name = parsedData["name"];
                        string password = parsedData["password"];
                        string captcha = parsedData["captcha"];
                        string captchaCode = context.Request.Cookies["captchaCode"]?.Value;

                        if (captchaCode == null || captcha != captchaCode)
                        {
                            ServeHtml(context, "register.html", "Invalid captcha.");
                            return;
                        }

                        if (_userRepository.CheckIfEmailExists(email))
                        {
                            ServeHtml(context, "register.html", "Email already exists.");
                        }
                        else
                        {
                            bool isRegistered = _userRepository.RegisterUser(email, name, password);
                            if (isRegistered)
                            {
                                User user = _userRepository.GetUserByEmail(email);
                                var cookie = new Cookie("userId", user.Id.ToString());
                                context.Response.Cookies.Add(cookie);
                                ServeHtml(context, "dashboard.html", $"Welcome, {user.Name}!", userId: user.Id.ToString(), username: user.Name);
                            }
                            else
                            {
                                ServeHtml(context, "register.html", "Registration failed. Please try again.");
                            }
                        }
                    }
                }
                else
                {
                    ServeHtml(context, "register.html");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
                ServeHtml(context, "register.html", "An error occurred during registration.");
            }
        }

        private void ServeHtml(HttpListenerContext context, string fileName, string message = null, string userId = null, string username = null)
        {
            Console.WriteLine($"Serving HTML file: {fileName}");
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views", fileName);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found, serving 404.");
                Serve404(context);
                return;
            }

            string content = File.ReadAllText(filePath);
            content = content.Replace("{{message}}", message ?? string.Empty)
                             .Replace("{{userId}}", userId ?? string.Empty)
                             .Replace("{{username}}", username ?? string.Empty);

            byte[] buffer = Encoding.UTF8.GetBytes(content);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
            Console.WriteLine("Response served and stream closed.");
        }

        private void Serve404(HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.StatusDescription = "Not Found";
            byte[] buffer = Encoding.UTF8.GetBytes("404 - Not Found");
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        private Dictionary<string, string> ParseFormData(string formData)
        {
            var parsedData = new Dictionary<string, string>();
            var pairs = formData.Split('&');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = WebUtility.UrlDecode(keyValue[0]);
                    var value = WebUtility.UrlDecode(keyValue[1]);
                    parsedData[key] = value;
                }
            }
            return parsedData;
        }

        public void HandleUpdate(HttpListenerContext context)
        {
            try
            {
                if (context.Request.HttpMethod == "POST")
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        string formData = reader.ReadToEnd();
                        var parsedData = ParseFormData(formData);
                        if (!parsedData.ContainsKey("userId") || string.IsNullOrEmpty(parsedData["userId"]))
                        {
                            throw new FormatException("User ID is missing or empty.");
                        }

                        int userId = int.Parse(parsedData["userId"]);
                        string email = parsedData["email"];
                        string name = parsedData["name"];
                        string password = parsedData.ContainsKey("password") ? parsedData["password"] : null;

                        User existingUser = _userRepository.GetUserByEmail(email);
                        if (existingUser != null && existingUser.Id != userId)
                        {
                            ServeHtml(context, "update.html", "Email is already taken by another user.", userId: parsedData["userId"]);
                            return;
                        }

                        bool isUpdated = _userRepository.UpdateUser(userId, email, name, string.IsNullOrEmpty(password) ? null : password);
                        if (isUpdated)
                        {
                            User updatedUser = _userRepository.GetUserById(userId);
                            ServeHtml(context, "dashboard.html", $"Profile updated successfully. Welcome, {updatedUser.Name}!", userId: parsedData["userId"], username: updatedUser.Name);
                        }
                        else
                        {
                            ServeHtml(context, "update.html", "Update failed. Please try again.", userId: parsedData["userId"]);
                        }
                    }
                }
                else
                {
                    string userId = context.Request.Cookies["userId"]?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        ServeHtml(context, "login.html", "Please log in to update your profile.");
                    }
                    else
                    {
                        ServeHtml(context, "update.html", userId: userId);
                    }
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Format error during profile update: {ex.Message}");
                ServeHtml(context, "update.html", "Invalid user ID format.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during profile update: {ex.Message}");
                ServeHtml(context, "update.html", "An error occurred during profile update.");
            }
        }

        public void HandleLogout(HttpListenerContext context)
        {
            try
            {
                if (context.Request.Cookies["userId"]?.Value != null)
                {
                    var cookie = new Cookie("userId", "")
                    {
                        Expires = DateTime.Now.AddDays(-1)
                    };
                    context.Response.Cookies.Add(cookie);
                }
                ServeHtml(context, "login.html", "Logout successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
                ServeHtml(context, "login.html", "An error occurred during logout.");
            }
        }

        public void HandleLogin(HttpListenerContext context)
        {
            try
            {
                if (context.Request.HttpMethod == "POST")
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        string formData = reader.ReadToEnd();
                        var parsedData = ParseFormData(formData);
                        string email = parsedData["email"];
                        string password = parsedData["password"];
                        string captcha = parsedData["captcha"];
                        string captchaCode = context.Request.Cookies["captchaCode"]?.Value;

                        if (captchaCode == null || captcha != captchaCode)
                        {
                            ServeHtml(context, "login.html", "Invalid captcha.");
                            return;
                        }

                        int userId = _authService.AuthenticateUser(email, password);
                        if (userId != -1)
                        {
                            User user = _userRepository.GetUserById(userId);
                            var cookie = new Cookie("userId", user.Id.ToString());
                            context.Response.Cookies.Add(cookie);
                            ServeHtml(context, "dashboard.html", $"Welcome, {user.Name}!", userId: user.Id.ToString(), username: user.Name);
                        }
                        else
                        {
                            ServeHtml(context, "login.html", "Invalid email or password.");
                        }
                    }
                }
                else
                {
                    ServeHtml(context, "login.html");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                ServeHtml(context, "login.html", "An error occurred during login.");
            }
        }

        public void HandleDashboard(HttpListenerContext context)
        {
            try
            {
                Console.WriteLine("Handling dashboard request...");
                if (context.Request.Cookies["userId"] != null)
                {
                    string userId = context.Request.Cookies["userId"].Value;
                    Console.WriteLine($"User ID from cookie: {userId}");
                    User user = _userRepository.GetUserById(int.Parse(userId));
                    if (user != null)
                    {
                        Console.WriteLine($"User found: {user.Name}");
                        ServeHtml(context, "dashboard.html", $"Welcome, {user.Name}!", userId, user.Name);
                    }
                    else
                    {
                        Console.WriteLine("User not found.");
                        ServeHtml(context, "login.html", "User not found. Please log in again.");
                    }
                }
                else
                {
                    Console.WriteLine("No user ID cookie found.");
                    ServeHtml(context, "login.html", "Please log in to access the dashboard.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during dashboard: {ex.Message}");
                ServeHtml(context, "dashboard.html", "An error occurred during dashboard.");
            }
        }

        public void HandleCaptcha(HttpListenerContext context)
        {
            string captchaCode = GenerateCaptchaCode();
            var captchaCookie = new Cookie("captchaCode", captchaCode)
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddMinutes(5)
            };
            context.Response.Cookies.Add(captchaCookie);

            context.Response.ContentType = "image/png";
            using (Bitmap bitmap = new Bitmap(100, 40))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.DrawString(captchaCode, new Font("Arial", 20), Brushes.Black, new PointF(10, 10));
                bitmap.Save(context.Response.OutputStream, ImageFormat.Png);
            }
            context.Response.OutputStream.Close();
        }

        private string GenerateCaptchaCode()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 5)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
