using ChatApp_Server.Classes;
using ChatApp_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp_Server
{
    internal class Program
    {
        static ushort port = 5000;
        static TcpListener listener;

        public static Dictionary<string, Client> clients;
        public static Dictionary<string, User> users;

        static void Main(string[] args)
        {

            if (args.Length > 0) ushort.TryParse(args[0], out port);
            // 데이터 베이스 접속
            if (Database.Connect())
            {
                Log("DB", $"Server {Database.hostname} is connected");
            }
            else
            {
                Log("DB", $"Server {Database.hostname} connect failed.");
                return;
            }

            // 서버 시작
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Log("System", $"Server is opened with port {port}");

            // 접속 받기
            while (true)
            {
                Client client = new Client(listener.AcceptTcpClient());
                if (!client.socket.Connected) continue;
                Log("Connect", "클라이언트 접속");
                
            }
        }

        public static void Log(string userId, string type, string content)
        {
            Console.WriteLine(string.Format("[{0}] {1} | {2} | {3}",
                DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
                userId, type, content));
        }

        public static void Log(string type, string content)
        {
            Console.WriteLine(string.Format("[{0}] {1} | {2}",
                DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
                type, content));
        }
        public static bool MoveLoginClient(Client c)
        {
            if (c.user.id == "")
            {
                Log("System", "사원 정보가 없는 클라이언트가 로그인을 했다고 함");
                return false;
            }

            // 변경점
            if (clients.ContainsKey(c.user.id))
                return false;

            clients.Add(c.user.id, c);
            //unloginedClients.Remove(c);
            return true;
        }
        public static void MoveLogoutClient(Client c)
        {
            if (c.user.id == "")
            {
                Log("System", "사원 정보가 없는 클라이언트가 로그아웃을 했다고 함");
                return;
            }

            //unloginedClients.Add(c);
            clients.Remove(c.user.id);
        }
    }
}
