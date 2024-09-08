using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppTelebid.Interfaces
{
    public interface IAuthService
    {
        int AuthenticateUser(string email, string password);
        string HashPassword(string password, byte[] salt);
    }
}
