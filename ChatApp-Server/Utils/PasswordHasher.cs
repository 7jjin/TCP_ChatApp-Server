using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp_Server.Utils
{
    internal class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashBytes); // Base64 문자열로 변환
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password); // 입력 비밀번호를 해시화
            return hashedInput == hashedPassword;     // 데이터베이스 값과 비교
        }
    }
}
