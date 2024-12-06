using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp_Server.Packets
{
    [Serializable]
    public class LogoutPacket : Packet
    {
        public LogoutPacket()
        {
            type = PacketType.Logout;
        }
    }
}
