using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp_Server.Packets
{
    public enum PacketType
    {
        Header, Close,
        Login, DuplicateLogin, Logout, Register,
        Chat, Room,
        Schedule,
    }
    [Serializable]
    public class Packet
    {
        public const int BUFFER_SIZE = 4096;

        public PacketType type;

        public Packet() { }
        public Packet(PacketType type)
        {
            this.type = type;
        }

        public byte[] Serialize() => Serialize(this);
        public static byte[] Serialize(object o)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, o);
                return ms.ToArray();
            }
        }
        public static object Deserialize(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer, 0, buffer.Length))
            {
                return new BinaryFormatter().Deserialize(ms);
            }
        }
    }
}
