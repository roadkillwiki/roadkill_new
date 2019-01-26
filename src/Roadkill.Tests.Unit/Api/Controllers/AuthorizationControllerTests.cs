using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NSubstitute;
using Roadkill.Api.Common.Models;
using Roadkill.Api.Controllers;
using Roadkill.Api.Settings;
using Roadkill.Core.Authorization;
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
		private UserManager<RoadkillUser> _userManagerMock;
		private SignInManager<RoadkillUser> _signinManagerMock;
		private JwtSettings _jwtSettings;

		public AuthorizationControllerTests()
		{
			_fixture = new Fixture();
			var fakeStore = Substitute.For<IUserStore<RoadkillUser>>();

			_userManagerMock = Substitute.For<UserManager<RoadkillUser>>(
				fakeStore,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				new NullLogger<UserManager<RoadkillUser>>());

			_signinManagerMock = Substitute.For<SignInManager<RoadkillUser>>(
				_userManagerMock,
				new Mock<IHttpContextAccessor>().Object,
				new Mock<IUserClaimsPrincipalFactory<RoadkillUser>>().Object,
				null,
				new NullLogger<SignInManager<RoadkillUser>>(),
				null);

			_jwtSettings = new JwtSettings()
			{
				Password = "this-password-should-be-over-18-characters",
				ExpireDays = 365
			};

			_authorizationController = new AuthorizationController(_userManagerMock, _signinManagerMock, _jwtSettings);
		}

		[Fact]
		public async Task GetAll_should_return_all_users_from_manager()
		{
			// given
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

			_userManagerMock.FindByEmailAsync(email)
				.Returns(Task.FromResult(roadkillUser));

			_signinManagerMock.PasswordSignInAsync(roadkillUser, password, true, false)
				.Returns(Task.FromResult(SignInResult.Success));

			var claims = new List<Claim>()
			{
				new Claim(ClaimTypes.Role, "Admin")
			};
			_userManagerMock.GetClaimsAsync(roadkillUser)
				.Returns(Task.FromResult(claims as IList<Claim>));

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
