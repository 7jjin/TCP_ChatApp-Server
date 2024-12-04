using ChatApp_Server.Models;
using ChatApp_Server.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChatApp_Server.Classes
{
    static class Database
    {
        public static SqlConnection con;
        public static readonly string hostname = "DESKTOP-B46DJQ2";
        public static readonly string dbName = "chatapp";
        public static readonly string id = "jin";
        public static readonly string password = "chatapp123";

        /// <summary>
        /// DB 연결
        /// </summary>
        /// <returns></returns>
        public static bool Connect()
        {
            con = new SqlConnection(string.Format($"Server={hostname};Database={dbName};User Id={id};Password={password};"));
            try
            {
                con.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 로그인 로직
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static User Login(string id, string password)
        {
            string command = "SELECT id, name, password, phoneNumber FROM users WHERE id = @id";
            SqlCommand cmd = new SqlCommand(command,con);
            cmd.Parameters.AddWithValue("@id", id);

            using (SqlDataReader reader = cmd.ExecuteReader()) 
                if(reader.Read())
                {
                    string storedPasswordHash = reader["password"].ToString();
                    // 암호화된 비밀번호와 비교 (비밀번호 검증 로직)
                    if (PasswordHasher.VerifyPassword(password, storedPasswordHash))
                        return new User(
                        reader["id"].ToString(),
                        reader["name"].ToString(),
                        storedPasswordHash,
                        reader["phoneNumber"].ToString()
                        );
                }
            return null;
        }

        // 회원가입
        public static bool Register(User user)
        {
            string command = "INSERT INTO users (id, name, password, phoneNumber) VALUES (@id, @name, @password, @phoneNumber)";
            SqlCommand cmd = new SqlCommand(command,con);
            cmd.Parameters.AddWithValue("@id", user.id);
            cmd.Parameters.AddWithValue("@name", user.name);
            cmd.Parameters.AddWithValue("@password", user.password);
            cmd.Parameters.AddWithValue("@phoneNumber", user.phoneNumber);

            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }catch(SqlException ex)
            {
                Program.Log("DB", ex.ToString());
                return false;
            }
        }

        // 사용 중인 유저
        public static Dictionary<string, User> GetUsers()
        {
            string command = "SELECT id, name, phoneNumber FROM users";
            SqlCommand cmd = new SqlCommand(command, con);
            Dictionary<string, User> users = new Dictionary<string, User>();

            using (SqlDataReader rdr = cmd.ExecuteReader())
                while (rdr.Read())
                {
                    users.Add((string)rdr["id"], new User(
                        (string)rdr["id"],
                        (string)rdr["name"],
                        string.Empty,
                        (string)rdr["phoneNumber"]));
                }
            return users;
        }

    }
}
