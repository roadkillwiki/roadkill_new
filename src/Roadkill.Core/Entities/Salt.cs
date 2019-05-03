using System;
using System.Text;

namespace Roadkill.Core.Entities
{
	/// <summary>
	///     Generates a random 16 character string for a hashed password salt.
	/// </summary>
	/// <remarks>
	///     password login:
	///     C1CD20DA5452C0D370794759CD151058AC189F2C
	///     1234567890
	/// </remarks>
	public class Salt
	{
		private static readonly Random _random = new Random();

		public string Value { get; }

		public Salt()
		{
			var builder = new StringBuilder(16);
			for (int i = 0; i < 16; i++)
			{
				char randomChar = (char)_random.Next(33, 126);
				builder.Append(randomChar);
			}

			Value = builder.ToString();
		}

		public static implicit operator string(Salt salt) => salt.Value;
		public override string ToString() => Value;
	}
}
