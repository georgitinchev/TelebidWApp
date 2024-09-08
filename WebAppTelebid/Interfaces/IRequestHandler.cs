using System.Net;
using WebAppTelebid.Interfaces;

namespace WebAppTelebid
{
    public interface IRequestHandler
    {
        void HandleRequest(HttpListenerContext context);
        void HandleRegister(HttpListenerContext context);
        void HandleLogin(HttpListenerContext context);
        void HandleUpdate(HttpListenerContext context);
        void HandleLogout(HttpListenerContext context);
        void HandleDashboard(HttpListenerContext context);
        void HandleCaptcha(HttpListenerContext context);
    }
}
