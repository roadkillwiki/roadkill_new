using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Roadkill.Api.Authorization.Policies;

namespace Roadkill.Api.Authorization.Roles
{
	public class RolesAuthorizationHandler : AuthorizationHandler<RoadkillPolicyRequirement>
	{
		private readonly IEnumerable<IUserRoleDefinition> _userRoles;

		public RolesAuthorizationHandler(IEnumerable<IUserRoleDefinition> userRoles)
		{
			_userRoles = userRoles;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoadkillPolicyRequirement requirement)
		{
			Claim claim = context.User.FindFirst(ClaimTypes.Role);
			if (claim != null && !string.IsNullOrEmpty(claim.Value))
			{
				foreach (IUserRoleDefinition userRoleDefinition in _userRoles)
				{
					if (userRoleDefinition.Name.Equals(claim.Value, StringComparison.OrdinalIgnoreCase))
					{
						if (userRoleDefinition.ContainsPolicy(requirement.PolicyName))
						{
							context.Succeed(requirement);
							break;
						}
					}

				}
			}

			return Task.CompletedTask;
		}
	}
}
