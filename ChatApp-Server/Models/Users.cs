using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp_Server.Models
{
    public class User
    {
        public string id, name, password, phoneNumber;

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
