using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppTelebid.Models;

namespace WebAppTelebid.Interfaces
{
    public interface IUserRepository
    {
        User GetUserById(int userId);
        bool RegisterUser(string email, string name, string password);
        bool UpdateUser(int userId, string email, string name, string password = null);
        bool CheckIfEmailExists(string email);
        User GetUserByEmail(string email);
    }
}
