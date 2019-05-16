using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Controllers;
using Roadkill.Api.JWT;
using Roadkill.Core.Authorization;
using Shouldly;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public sealed class AuthorizationControllerTests
	{
		private AuthorizationController _authorizationController;
		private UserManager<RoadkillIdentityUser> _userManagerMock;
		private SignInManager<RoadkillIdentityUser> _signinManagerMock;
		private IJwtTokenProvider _jwtTokenProvider;

		public AuthorizationControllerTests()
		{
			var userManagerLogger = new NullLogger<UserManager<RoadkillIdentityUser>>();
			var userStoreMock = Substitute.For<IUserStore<RoadkillIdentityUser>>();
			_userManagerMock = Substitute.For<UserManager<RoadkillIdentityUser>>(
				userStoreMock,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				userManagerLogger);

			var signinManagerLogger = new NullLogger<SignInManager<RoadkillIdentityUser>>();
			var contextAccessorMock = Substitute.For<IHttpContextAccessor>();
			var claimFactoryMock = Substitute.For<IUserClaimsPrincipalFactory<RoadkillIdentityUser>>();
			_signinManagerMock = Substitute.For<SignInManager<RoadkillIdentityUser>>(
				_userManagerMock,
				contextAccessorMock,
				claimFactoryMock,
				null,
				signinManagerLogger,
				null);

			_jwtTokenProvider = Substitute.For<IJwtTokenProvider>();
			_authorizationController = new AuthorizationController(_userManagerMock, _signinManagerMock, _jwtTokenProvider);
		}

		[Fact]
		public void Authenticate_should_be_HttpPost_and_have_route_template()
		{
			string methodName = nameof(AuthorizationController.Authenticate);
			Type attributeType = typeof(HttpPostAttribute);

			_authorizationController.ShouldHaveAttribute(methodName, attributeType);
			_authorizationController.ShouldHaveRouteAttributeWithTemplate(methodName, methodName);
		}

		[Fact]
		public void Authenticate_should_allow_anonymous()
		{
			_authorizationController.ShouldAllowAnonymous(nameof(AuthorizationController.Authenticate));
		}

		[Fact]
		public async Task Authenticate_should_return_token_from_provider()
		{
			// given
			string jwtToken = "jwt token";
			string email = "admin@example.org";
			string password = "Passw0rd9000!";
			var roadkillUser = new RoadkillIdentityUser()
			{
				Id = "1",
				UserName = email,
				NormalizedUserName = email.ToUpperInvariant(),
				Email = email,
				NormalizedEmail = email.ToUpperInvariant(),
				RoleClaims = new List<string>()
			};

			var model = new AuthorizationRequest()
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

			var model = new AuthorizationRequest()
			{
				Email = email,
				Password = password
			};

			_userManagerMock.FindByEmailAsync(email)
				.Returns(Task.FromResult((RoadkillIdentityUser)null));

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
			var roadkillUser = new RoadkillIdentityUser()
			{
				Id = "1",
				UserName = email,
				NormalizedUserName = email.ToUpperInvariant(),
				Email = email,
				NormalizedEmail = email.ToUpperInvariant()
			};

			var model = new AuthorizationRequest()
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
