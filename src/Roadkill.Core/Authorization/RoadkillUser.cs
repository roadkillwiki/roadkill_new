using System.Collections.Generic;
using Marten.AspNetIdentity;
using Microsoft.AspNetCore.Identity;

namespace Roadkill.Core.Authorization
{
	public class RoadkillUser : IdentityUser, IClaimsUser
	{
		public IList<byte[]> Claims { get; set; }
	}
}
