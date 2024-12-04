using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp_Server.Models
{
    public class User
    {
      
        public string id { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string phoneNumber { get; set; }

        public User() { }
        public User(string id, string password)
        {
            this.id = id;
            this.password = password;
        }

        public User(string id, string name, string password, string phoneNumber)
        {
            this.id = id;
            this.name = name;
            this.password = password;
            this.phoneNumber = phoneNumber;
        }
    }
}
