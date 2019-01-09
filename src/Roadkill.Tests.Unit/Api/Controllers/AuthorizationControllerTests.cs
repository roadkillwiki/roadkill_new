using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Roadkill.Api.Common.Models;
using Roadkill.Api.Controllers;
using Roadkill.Core.Authorization;
using Shouldly;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public sealed class AuthorizationControllerTests
	{
		private readonly Fixture _fixture;
		private AuthorizationController _authorizationController;
		private UserManager<RoadkillUser> _userManagerMock;
		private SignInManager<RoadkillUser> _signinManagerMock;

		public AuthorizationControllerTests()
		{
			_fixture = new Fixture();
			_userManagerMock = Substitute.For<UserManager<RoadkillUser>>();
			_signinManagerMock = Substitute.For<SignInManager<RoadkillUser>>();

			_authorizationController = new AuthorizationController(_userManagerMock, _signinManagerMock);
		}

		[Fact]
		public async Task GetAll_should_return_all_users_from_manager()
		{
			// given
			var signInResult = SignInResult.Success;
			string email = "admin@example.org";
			string password = "labrador";
			var roadkillUser = new RoadkillUser()
			{
				Email = email,
				NormalizedEmail = email.ToUpperInvariant()
			};

			var model = new AuthenticationModel()
			{
				Email = email,
				Password = password
			};

			var claimsList = new List<Claim>()
			{
				new Claim(ClaimTypes.Role, "Admin")
			};

			_userManagerMock.FindByEmailAsync(email).Returns(roadkillUser);
			_signinManagerMock.PasswordSignInAsync(roadkillUser, password, true, false).Returns(signInResult);
			_userManagerMock.GetClaimsAsync(roadkillUser).Returns(claimsList);

			// Inject SecurityTokenHandler

			// when
			OkObjectResult actionResult = await _authorizationController.Authenticate(model) as OkObjectResult;

			// then
			actionResult.ShouldNotBeNull();
			actionResult.Value.ShouldBeOfType<string>();
			actionResult.Value.ToString().ShouldNotBeNullOrWhiteSpace();
		}
	}
}
