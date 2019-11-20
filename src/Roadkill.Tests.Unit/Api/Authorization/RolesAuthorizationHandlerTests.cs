using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using Roadkill.Api.Authorization;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Authorization
{
	public class RolesAuthorizationHandlerTests
	{
		private readonly RolesAuthorizationHandler _rolesAuthorizationHandler;
		private readonly List<IUserRoleDefinition> _userRoleDefinitions;

		public RolesAuthorizationHandlerTests()
		{
			_userRoleDefinitions = new List<IUserRoleDefinition>();
			_rolesAuthorizationHandler = new RolesAuthorizationHandler(_userRoleDefinitions);
		}

		[Fact]
		public async Task should_succeed_for_role_with_policy_in_it()
		{
			// given
			string authorizedRoleName = "mega.user";
			string policyInRole = "DeletePage";

			IAuthorizationRequirement requirement = AddMockRoleAndCreateRequirement(authorizedRoleName, policyInRole);
			var requirements = new List<IAuthorizationRequirement>();
			requirements.Add(requirement);

			ClaimsPrincipal user = CreateClaimsPrincipal(authorizedRoleName);

			var context = new AuthorizationHandlerContext(requirements, user, null);

			// when
			await _rolesAuthorizationHandler.HandleAsync(context);

			// then
			context.HasSucceeded.ShouldBeTrue();
		}

		[Fact]
		public async Task should_fail_for_role_without_policy_in_it()
		{
			// given
			string authorizedRoleName = "basic.user";
			string policyInRole = "AddPage";
			IAuthorizationRequirement basicUserRequirement = AddMockRoleAndCreateRequirement(authorizedRoleName, policyInRole);

			string adminRoleName = "admin.rooty";
			string policyInAdminRole = "DeletePage";
			IAuthorizationRequirement adminUserRequirement = AddMockRoleAndCreateRequirement(adminRoleName, policyInAdminRole);

			var requirements = new List<IAuthorizationRequirement>();
			requirements.Add(basicUserRequirement);
			requirements.Add(adminUserRequirement);

			ClaimsPrincipal user = CreateClaimsPrincipal(authorizedRoleName);

			var context = new AuthorizationHandlerContext(requirements, user, null);

			// when
			await _rolesAuthorizationHandler.HandleAsync(context);

			// then
			context.HasSucceeded.ShouldBeFalse();
		}

		private IAuthorizationRequirement AddMockRoleAndCreateRequirement(string roleName, string policyName)
		{
			var mockRoleDefinition = Substitute.For<IUserRoleDefinition>();
			mockRoleDefinition.Name.Returns(roleName);
			mockRoleDefinition.ContainsPolicy(policyName).Returns(true);
			_userRoleDefinitions.Add(mockRoleDefinition);

			return new RoadkillPolicyRequirement(policyName);
		}

		private ClaimsPrincipal CreateClaimsPrincipal(string roleName)
		{
			var claims = new List<Claim>()
			{
				new Claim(ClaimTypes.Role, roleName)
			};
			var claimsIdentities = new List<ClaimsIdentity> {new ClaimsIdentity(claims)};
			return new ClaimsPrincipal(claimsIdentities);
		}
	}
}
