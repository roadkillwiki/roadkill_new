using System.Collections.Generic;
using Marten.AspNetIdentity;
using Microsoft.AspNetCore.Identity;

namespace Roadkill.Core.Entities.Authorization
{
	public class RoadkillIdentityUser : IdentityUser, IClaimsUser
	{
		public IList<string> RoleClaims { get; set; }
	}
}
