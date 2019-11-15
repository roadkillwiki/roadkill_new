using Microsoft.AspNetCore.Authorization;

namespace Roadkill.Api.Authorization
{
	public class RoadkillUserRequirement : IAuthorizationRequirement
	{
		public string RoleName { get; set; }
		public RoadkillUserRequirement(string roleName)
		{
			RoleName = roleName;
		}
	}
}
