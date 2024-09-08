using WebAppTelebid.Interfaces;

namespace WebAppTelebid
{
    public interface IRequestHandler
    {
        void HandleRequest(IHttpListenerContext context);
        void HandleRegister(IHttpListenerContext context);
        void HandleLogin(IHttpListenerContext context);
        void HandleUpdate(IHttpListenerContext context);
        void HandleLogout(IHttpListenerContext context);
        void HandleDashboard(IHttpListenerContext context);
        void HandleCaptcha(IHttpListenerContext context);
    }
}
