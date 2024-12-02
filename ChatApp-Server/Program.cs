using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp_Server
{
    internal class Program
    {
        static void Main()
        {
            Server server = new Server();
            server.StartServer(5000); // 서버 포트: 5000
        }
    }
}
