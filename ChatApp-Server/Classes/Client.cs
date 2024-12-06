using ChatApp_Server.Models;
using ChatApp_Server.Packets;
using ChatApp_Server.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp_Server.Classes
{
    public class Client
    {
        public TcpClient socket;    // 클라이언트 소켓
        NetworkStream ns;           // 네트워크 스트림
        public Thread recvThread;   // 클라이언트로부터 수신을 대기하는 스레드
        public User user;           // 클라이언트의 유저 정보

        public Client(TcpClient socket)
        {
            this.socket = socket;
            ns = socket.GetStream();
            user = new User(string.Empty, string.Empty);

            recvThread = new Thread(new ThreadStart(Recieve));
            recvThread.Start();
        }

        /// <summary>
        /// 패킷 수신
        /// </summary>
        public void Recieve()
        {
            byte[] readBuffer = null, lengthBuffer = new byte[4];
            while (true)
            {
                // 패킷 읽기
                try
                {
                    // 패킷 길이 읽기
                    if (ns.Read(lengthBuffer, 0, 4) < 4)
                    {
                        Log("Receive", "패킷 길이를 읽지 못하였습니다.");

                        // 수신 버퍼 리셋
                        while (ns.ReadByte() != -1) ;
                    }
                    readBuffer = new byte[BitConverter.ToInt32(lengthBuffer, 0)];
                    int position = 0;

                    // 길이를 토대로 데이터 읽기 잘려서 와도 끝까지 읽음
                    while (position < readBuffer.Length)
                    {
                        position += ns.Read(readBuffer, position, readBuffer.Length - position);
                    }
                }
                catch (IOException socketEx)
                {
                    Log("IOException", socketEx.Message);
                    //Program.RemoveClient(this);
                    break;
                }
                catch (Exception ex)
                {
                    Log("Exception", ex.ToString());
                    //Program.RemoveClient(this);
                    break;
                }

                // 패킷 번역
                try
                {
                    // AES256 해독 후 JSON 기반 역직렬화
                    string decryptedJson = Encoding.UTF8.GetString(AES256.Decrypt(readBuffer));
                    Packet packet = JsonSerializer.Deserialize<Packet>(decryptedJson);

                    // 역직렬화 후 타입 확인
                    if (packet == null)
                    {
                        Log("Deserialize", "패킷 역직렬화 실패");
                        continue;
                    }

                    // 연결
                    if (packet.type == PacketType.Header)
                    {
                        Log("Warning", "Received no length HeaderPacket");
                    }
                    else if (packet.type == PacketType.Close)
                    {
                        Log("Close", "Disconnect client");
                        socket.Close();
                        break;
                    }
                    // 로그인
                    else if (packet.type == PacketType.Login)
                    {
                        // 로그인 패킷 역직렬화
                        LoginPacket loginPacket = JsonSerializer.Deserialize<LoginPacket>(decryptedJson);
                        User loginUser = Database.Login(loginPacket.users[0].id, loginPacket.users[0].password);
                        if (user == null)
                        {
                            Log("Login", $"{loginPacket.users[default].id} 로그인 실패");
                            loginPacket.success = false;
                        }
                        else
                        {
                            Log("Login", $"{loginPacket.users[default].id} 로그인 성공");
                            loginPacket.success = true;
                        }


                        Thread.Sleep(200);
                        Send(loginPacket);
                    }
                    // 로그아웃
                    else if (packet.type == PacketType.Logout)
                    {
                        Program.MoveLogoutClient(this);
                        Log("Logout", "로그아웃");
                        user = new User(string.Empty, string.Empty);
                    }
                    // 회원가입
                    else if (packet.type == PacketType.Register)
                    {
                        RegisterPacket registerPacket = JsonSerializer.Deserialize<RegisterPacket>(decryptedJson);
                        if (registerPacket.success = Database.Register(registerPacket.user))
                        {
                            Program.users.Add(registerPacket.user.id, user = registerPacket.user);
                            Log("Register", "회원가입 성공");
                        }
                        else
                        {
                            Log("Register", "회원가입 실패");
                        }

                        Send(registerPacket);
                    }
                    // 아이디 중복 체크
                    else if (packet.type == PacketType.IdCheck)
                    {
                        IdCheckPacket idCheckPacket = JsonSerializer.Deserialize<IdCheckPacket>(decryptedJson);
                        if (!Database.IdCheck(idCheckPacket.id))
                        {
                            idCheckPacket.success = true;
                            Log("Register", "회원가입 성공");
                        }
                        else
                        {
                            idCheckPacket.success = false;
                            Log("Register", "회원가입 실패");
                        }

                        Send(idCheckPacket);
                    }
                }
                catch (Exception ex)
                {
                    Log("Deserialize", ex.ToString());

                    // 수신 버퍼 리셋
                    while (ns.ReadByte() != -1) ;
                    continue;
                }




            }
        }

        /// <summary>
        /// Packet 전송
        /// </summary>
        /// <param name="packet"></param>
        public void Send(Packet packet)
        {
            lock (this)
            {
                byte[] sendBuffer = AES256.Encrypt(packet.Serialize());
                byte[] lengthBuffer = BitConverter.GetBytes(sendBuffer.Length);

                ns.Write(lengthBuffer, 0, 4);
                ns.Write(sendBuffer,0,sendBuffer.Length);
                ns.Flush();
            }
        }

        /// <summary>
        /// 로그 기록 확인
        /// </summary>
        /// <param name="type"></param>
        /// <param name="content"></param>
        void Log(string type, string content)
        {
            Program.Log(user.id, type, content);
        }
    }
}
