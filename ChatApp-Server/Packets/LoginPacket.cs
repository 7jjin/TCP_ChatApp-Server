using ChatApp_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp_Server.Packets
{
    [Serializable]
    public class LoginPacket : Packet
    {
        public Dictionary<string, User> users;

        public bool success = false;

        public LoginPacket(string id, string password)
        {
            type = PacketType.Login;
            users = new Dictionary<string, User>();
            users.Add("default", new User(id, password));
        }

        public LoginPacket(bool success, Dictionary<string, User> users)
        {
            type = PacketType.Login;

            this.success = success;
            this.users = users;
        }

    }
}
