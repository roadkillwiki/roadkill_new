using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Roadkill.Api.Authorization
{
	public class RoadkillAuthorizer : AuthorizationHandler<RoadkillUserRequirement>
	{
		private readonly IEnumerable<IUserRoleDefinition> _userRoles;

		public RoadkillAuthorizer(IServiceProvider serviceProvider)
		{
			_userRoles = serviceProvider.GetServices<IUserRoleDefinition>();
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoadkillUserRequirement requirement)
		{
			Claim claim = context.User.FindFirst(ClaimTypes.Role);
			if (claim != null && !string.IsNullOrEmpty(claim.Value))
			{
				foreach (IUserRoleDefinition userRoleDefinition in _userRoles)
				{
					if (userRoleDefinition.ContainsClaim(claim.Value))
					{
						context.Succeed(requirement);
						break;
					}
				}
			}
			return Task.CompletedTask;
		}
	}
}
