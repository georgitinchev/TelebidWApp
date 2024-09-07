using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppTelebid.Interfaces
{
    public interface IDatabaseHelper
    {
        SqlConnection GetConnection();
        void OpenConnection(SqlConnection connection);
        void CloseConnection(SqlConnection connection);
    }
}
