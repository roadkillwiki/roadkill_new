using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NSubstitute;
using Roadkill.Api.Common.Models;
using Roadkill.Api.Controllers;
using Roadkill.Api.JWT;
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
		private AuthorizationController _authorizationController;
		private UserManager<RoadkillUser> _userManagerMock;
		private SignInManager<RoadkillUser> _signinManagerMock;
		private IJwtTokenProvider _jwtTokenProvider;

		public AuthorizationControllerTests()
		{
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

			_jwtTokenProvider = Substitute.For<IJwtTokenProvider>();
			_authorizationController = new AuthorizationController(_userManagerMock, _signinManagerMock, _jwtTokenProvider);
		}

		[Fact]
		public async Task Authenticate_should_return_token_from_provider()
		{
			// given
			string jwtToken = "jwt token";
			string email = "admin@example.org";
			string password = "Passw0rd9000!";
			var roadkillUser = new RoadkillUser()
			{
				Id = "1",
				UserName = email,
				NormalizedUserName = email.ToUpperInvariant(),
				Email = email,
				NormalizedEmail = email.ToUpperInvariant(),
				RoleClaims = new List<string>()
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

			var claims = new List<Claim>() { new Claim("any", "thing") } as IList<Claim>;
			_userManagerMock.GetClaimsAsync(roadkillUser)
				.Returns(Task.FromResult(claims));

			_jwtTokenProvider.CreateToken(claims, roadkillUser.Email)
				.Returns(jwtToken);

			// when
			ActionResult<string> actionResult = await _authorizationController.Authenticate(model);

			// then
			actionResult.Result.ShouldBeOfType<OkObjectResult>();

			var okResult = actionResult.Result as OkObjectResult;
			okResult.Value.ShouldBe(jwtToken);
		}

		[Fact]
		public async Task Authenticate_should_return_notfound_if_user_is_not_found()
		{
			// given
			string email = "admin@example.org";
			string password = "Passw0rd9000";

			var model = new AuthenticationModel()
			{
				Email = email,
				Password = password
			};

			_userManagerMock.FindByEmailAsync(email)
				.Returns(Task.FromResult((RoadkillUser)null));

			// when
			ActionResult<string> actionResult = await _authorizationController.Authenticate(model);

			// then
			actionResult.ShouldNotBeNull();
			actionResult.Result.ShouldBeOfType<NotFoundObjectResult>();
		}

		[Fact]
		public async Task Authenticate_should_return_forbidden_when_signin_fails()
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
				.Returns(Task.FromResult(SignInResult.Failed));

			// when
			ActionResult<string> actionResult = await _authorizationController.Authenticate(model);

			// then
			actionResult.ShouldNotBeNull();
			actionResult.Result.ShouldBeOfType<ForbidResult>();
		}
	}
}
