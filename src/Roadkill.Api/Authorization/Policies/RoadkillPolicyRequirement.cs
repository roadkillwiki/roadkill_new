using Microsoft.AspNetCore.Authorization;

namespace Roadkill.Api.Authorization.Policies
{
	public class RoadkillPolicyRequirement : IAuthorizationRequirement
	{
		public string PolicyName { get; set; }
		public RoadkillPolicyRequirement(string policyName)
		{
			PolicyName = policyName;
		}
	}
}
