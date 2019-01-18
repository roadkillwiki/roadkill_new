using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Roadkill.Api.Common.Models;
using Roadkill.Api.Controllers;
using Roadkill.Api.Settings;
using Roadkill.Core.Authorization;
using Roadkill.Tests.Unit.Mocks;
using Shouldly;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	[SuppressMessage("Stylecop", "CA1063", Justification = "IDisposable overkill")]
	[SuppressMessage("Stylecop", "CA1001", Justification = "IDisposable overkill")]
	public sealed class AuthorizationControllerTests
	{
		private readonly Fixture _fixture;
		private AuthorizationController _authorizationController;
		private MockUserStore _mockUserStore;
		private UserManager<RoadkillUser> _userManagerMock;
		private SignInManager<RoadkillUser> _signinManagerMock;
		private JwtSettings _jwtSettings;

		public AuthorizationControllerTests()
		{
			_fixture = new Fixture();
			_mockUserStore = new MockUserStore();
			_userManagerMock = MockIdentityManagersFactory.CreateUserManager(_mockUserStore);
			_signinManagerMock = MockIdentityManagersFactory.CreateSigninManager(_userManagerMock);
			_jwtSettings = new JwtSettings();

			_authorizationController = new AuthorizationController(_userManagerMock, _signinManagerMock, _jwtSettings);
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
