using System;
using Marten.Schema;

namespace Roadkill.Core.Authorization
{
	public class UserRefreshToken
	{
		[Identity()]
		public string Email { get; set; }

		public string RefreshToken { get; set; }

		public DateTime CreationDate { get; set; }
	}
}
