using System;
using Marten.Schema;

namespace Roadkill.Core.Entities.Authorization
{
	public class UserRefreshToken
	{
		[Identity]
		public string Id { get; set; }

		public string Email { get; set; }
		public string RefreshToken { get; set; }
		public DateTime CreationDate { get; set; }
		public string IpAddress { get; set; }
	}
}
