using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebAppTelebid
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://loccalhost:8080/");
            listener.Start();
            Console.WriteLine("Server started at port 8080");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "GET" && request.RawUrl == "/")
                {
                    string responseString = File.ReadAllText("index.html");
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                else if (request.HttpMethod == "POST" && request.RawUrl == "/register")
                {
                    // Handle form submission for registration here
                    // Read form data, validate, save to database
                }
                else if (request.HttpMethod == "POST" && request.RawUrl == "/login")
                {
                    // Handle form submission for login here
                    // Read form data, validate, check against database
                }
                else if (request.HttpMethod == "GET" && request.RawUrl == "/logout")
                {
                    // Handle logout here
                    // Clear session data, redirect to login page
                }
                else
                {
                    response.StatusCode = 404;
                    response.StatusDescription = "Not Found";
                    response.Close();
                }
            }
        }

    }
}
