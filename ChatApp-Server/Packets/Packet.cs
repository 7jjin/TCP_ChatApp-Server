using Newtonsoft.Json;
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

        public PacketType type { get; set; }

        public Packet()
        {

        }
        public Packet(PacketType type)
        {
            this.type = type;
        }

        public byte[] Serialize() => Serialize(this);
        public static byte[] Serialize(object o)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                string json = JsonConvert.SerializeObject(o); // 객체를 JSON 문자열로 변환
                return Encoding.UTF8.GetBytes(json); // JSON 문자열을 바이트 배열로 변환
            }
        }
        public static T Deserialize<T>(byte[] buffer)
        {
            string json = Encoding.UTF8.GetString(buffer); // 바이트 배열을 JSON 문자열로 변환
            return JsonConvert.DeserializeObject<T>(json); // JSON 문자열을 객체로 역직렬화
        }
        public static object Deserialize(byte[] buffer, Type targetType)
        {
            string jsonString = Encoding.UTF8.GetString(buffer);
            return JsonConvert.DeserializeObject(jsonString);
        }
    }
}
