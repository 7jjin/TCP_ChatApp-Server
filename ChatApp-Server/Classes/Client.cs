﻿using ChatApp_Server.Models;
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
                object packetObj = null;
                try
                {
                    packetObj = Packet.Deserialize(AES256.Decrypt(readBuffer));
                }
                catch (Exception ex)
                {
                    {
                        Log("Deserialize", ex.ToString());
                        // 수신 버퍼 리셋
                        while (ns.ReadByte() != 1) ;
                    }
                    if (packetObj == null) continue;

                    Packet packet = packetObj as Packet;

                    // 연결
                    if (packet.type == PacketType.Header)
                    {
                        Log("Waring", "Receieved no length HeaderPacket");
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
                        LoginPacket p = packet as LoginPacket;
                        // 변경점
                        User user = Database.Login(p.users[default].id, p.users[default].password);
                        if (user != null)
                        {
                            Log("Login", string.Format("{0} 로그인 실패", p.users[default].id));
                            p.success = false;
                        }
                        else
                        {
                            Log("Login", "로그인 성공");
                            p = new LoginPacket(true, Program.users);
                        }
                        Thread.Sleep(200);
                        Send(p);
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
                        RegisterPacket p = packet as RegisterPacket;
                        if (p.success = Database.Register(p.user))
                        {
                            Program.users.Add(p.user.id, user = p.user);
                            Log("Register", "회원가입 성공");
                        }
                        else
                            Log("Register", "회원가입 실패");
                        Send(p);
                    }

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