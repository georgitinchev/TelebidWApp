using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using WebAppTelebid.Interfaces;
using WebAppTelebid.Repositories;
using WebAppTelebid.Services;

namespace WebAppTelebid
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string connectionString = LoadConnectionString();
            IDatabaseHelper databaseHelper = new DatabaseHelper(connectionString);
            IUserRepository userRepository = new UserRepository(databaseHelper);
            IAuthService authService = new AuthService(userRepository);
            RequestHandler requestHandler = new RequestHandler(authService, userRepository);
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            Console.WriteLine("Server is running on http://localhost:8080/");

            while (true)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    requestHandler.HandleRequest(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private static string LoadConnectionString()
        {
            var jsonString = File.ReadAllText("appsettings.json");
            var jsonDocument = JsonDocument.Parse(jsonString);
            return jsonDocument.RootElement
                .GetProperty("ConnectionStrings")
                .GetProperty("DefaultConnection")
                .GetString();
        }
    }
}
