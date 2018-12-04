using System;
using System.Security.Cryptography;
using System.Text;

namespace Roadkill.Core.Entities
{
    public class User
    {
        public string ActivationKey { get; set; }

        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Firstname { get; set; }

        public bool IsEditor { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsActivated { get; set; }

        public string Lastname { get; set; }

        public string Password { get; internal set; }

        public string PasswordResetKey { get; set; }

        public string Salt { get; set; }

        public string Username { get; set; }

        public void SetPassword(string password)
        {
            // Encrypts and sets the password for the user.
            Salt = new Salt();
            Password = HashPassword(password, Salt);
        }

        public static string HashPassword(string password, string salt)
        {
            SHA256 sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(password + salt));

            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// Generates a random 16 character string for a hashed password salt.
    /// </summary>
    /// <remarks>
    /// password login:
    /// C1CD20DA5452C0D370794759CD151058AC189F2C
    /// 1234567890
    /// </remarks>
    public class Salt
    {
        private static Random _random = new Random();

        /// <summary>
        /// The salt value.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Salt"/> class, generating a new salt value.
        /// </summary>
        public Salt()
        {
            StringBuilder builder = new StringBuilder(16);
            for (int i = 0; i < 16; i++)
            {
                builder.Append((char)_random.Next(33, 126));
            }

            Value = builder.ToString();
        }

        public static implicit operator string(Salt salt)
        {
            return salt.Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}