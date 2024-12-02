using ChatApp_Server.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp_Server
{
    class Server
    {
        private TcpListener _listener;
        // DB 설정
        private string _connectionString = "Server='DESKTOP-B46DJQ2';Database='chatapp';User Id='jin';Password='chatapp123';";

        /// <summary>
        /// Server 실행
        /// </summary>
        /// <param name="port"></param>
        public void StartServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Console.WriteLine("Server Started...");

            while (true)
            {
                var client = _listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }


        private void HandleClient(object clientObj)
        {
            using (var client = (TcpClient)clientObj)
            using (var stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];

                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);


                string response = ProcessRequest(request);
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);

                stream.Write(responseBytes, 0, responseBytes.Length);
            }
        }

        private string ProcessRequest(string request)
        {
            try
            {
                // client가 전송한 데이터 확인
                Console.Write(request);
                var parts = request.Split('|');
                string command = parts[0];

                if (command == "REGISTER")
                {
                    string username = parts[1];
                    string password = parts[2];
                    return RegisterUser(username, password);
                }
                else if (command == "LOGIN")
                {
                    string username = parts[1];
                    string password = parts[2];
                    return AuthenticateUser(username, password);
                }
                return "ERROR|Invalid command";
            }
            catch
            {
                return "ERROR|Invalid request format";
            }
        }

        private string RegisterUser(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string hash = BCrypt.Net.BCrypt.HashPassword(password); ;

                var command = new SqlCommand("INSERT INTO users (username,password) VALUSE (@username,@password)", connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", hash);

                try
                {
                    command.ExecuteNonQuery();
                    return "SUCCESS|User registered";
                }
                catch (SqlException)
                {
                    return "ERROR|Username already exists";
                }
            }
        }

        private string AuthenticateUser(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT password FROM users WHERE id = @username", connection);
                command.Parameters.AddWithValue("@username", username);

                try
                {
                    // SELECT 쿼리 결과에서 첫 번째 열의 값을 가져옴
                    var dbPassword = command.ExecuteScalar() as string;

                    if (dbPassword != null && PasswordHasher.VerifyPassword(password, dbPassword))
                    {
                        return "SUCCESS|User LOGIN";
                    }
                    else
                    {
                        return "ERROR|Invalid username or password";
                    }
                }
                catch (SqlException)
                {
                    return "ERROR|Username already exists";
                }
            }
        }

    }

    
}
