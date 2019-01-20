using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading;
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
		private MockUserStore<RoadkillUser> _mockUserStore;
		private UserManager<RoadkillUser> _userManagerMock;
		private SignInManager<RoadkillUser> _signinManagerMock;
		private JwtSettings _jwtSettings;

		public AuthorizationControllerTests()
		{
			_fixture = new Fixture();
			_mockUserStore = new MockUserStore<RoadkillUser>();
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
			string password = "Passw0rd9000";
			var roadkillUser = new RoadkillUser()
			{
				Id = "1",
				UserName = email,
				NormalizedUserName = email.ToUpperInvariant(),
				Email = email,
				NormalizedEmail = email.ToUpperInvariant()
			};

			var model = new AuthenticationModel()
			{
				Email = email,
				Password = password
			};

			await _userManagerMock.CreateAsync(roadkillUser, password);
			await _userManagerMock.AddClaimAsync(roadkillUser, new Claim(ClaimTypes.Role, "Admin"));

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
