using System;
using System.Globalization;
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

		private static string HashPassword(string password, string salt)
		{
			SHA256 sha = new SHA256Managed();
			var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(password + salt));

			var stringBuilder = new StringBuilder();
			foreach (byte b in hash)
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
			}

			return stringBuilder.ToString();
		}
	}
}
