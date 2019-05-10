using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.Controllers;
using Roadkill.Api.JWT;
using Roadkill.Api.ObjectConverters;
using Roadkill.Core.Authorization;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public sealed class UsersControllerTests
	{
		private readonly Fixture _fixture;
		private UsersController _usersController;
		private UserManager<RoadkillIdentityUser> _userManagerMock;
		private IUserObjectsConverter _objectConverterMock;

		public UsersControllerTests()
		{
			_fixture = new Fixture();
			var fakeStore = Substitute.For<IUserStore<RoadkillIdentityUser>>();

			_userManagerMock = Substitute.For<UserManager<RoadkillIdentityUser>>(
				fakeStore,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				new NullLogger<UserManager<RoadkillIdentityUser>>());

			_objectConverterMock = Substitute.For<IUserObjectsConverter>();
			_usersController = new UsersController(_userManagerMock, _objectConverterMock);
		}

		[Fact]
		public async Task GetByEmail_should_return_user()
		{
			// given
			string email = "donny@trump.com";
			var identityUser = new RoadkillIdentityUser() { Email = email };
			var responseUser = new UserResponse() { Email = email };

			_userManagerMock.FindByEmailAsync(email)
				.Returns(identityUser);

			_objectConverterMock
				.ConvertToUserResponse(Arg.Any<RoadkillIdentityUser>())
				.Returns(responseUser);

			// when
			var actionResult = await _usersController.GetByEmail(email);

			// then
			actionResult.ShouldBeOkObjectResult();
			UserResponse actualUser = actionResult.GetOkObjectResultValue();
			actualUser.Email.ShouldBe(email);
		}

		[Fact]
		public async Task GetByEmail_should_return_notfound_when_user_doesnt_exist()
		{
			// given
			string email = "okfingers@trump.com";
			var expectedError = UsersController.EmailDoesNotExistError;

			_userManagerMock.FindByEmailAsync(email)
				.Returns(Task.FromResult((RoadkillIdentityUser)null));

			// when
			ActionResult<UserResponse> actionResult = await _usersController.GetByEmail(email);

			// then
			actionResult.ShouldBeNotFoundObjectResult();
			string errorMessage = actionResult.GetNotFoundValue<UserResponse, string>();
			errorMessage.ShouldBe(expectedError);
		}

		[Fact]
		public void FindAll_should_return_all_users_from_manager()
		{
			// given
			var expectedAllUsers = _fixture.CreateMany<RoadkillIdentityUser>(5);
			_userManagerMock.Users.Returns(expectedAllUsers.AsQueryable());

			// when
			var actionResult = _usersController.FindAll();

			// then
			actionResult.ShouldBeOkObjectResult();
			IEnumerable<UserResponse> actualUsers = actionResult.GetOkObjectResultValue();
			actualUsers.Count().ShouldBe(expectedAllUsers.Count());
		}

		[Fact]
		public async Task FindUsersWithClaim_should_return_specific_users_for_claim()
		{
			// given
			string claimName = ClaimTypes.Role;
			string claimValue = RoleNames.Admin;

			var expectedUsers = new List<RoadkillIdentityUser>()
			{
				_fixture.Create<RoadkillIdentityUser>(),
				_fixture.Create<RoadkillIdentityUser>()
			};

			_userManagerMock
				.GetUsersForClaimAsync(Arg.Is<Claim>(c =>
					c.Type == claimName && c.Value == claimValue))
				.Returns(Task.FromResult((IList<RoadkillIdentityUser>)expectedUsers));

			// when
			var actionResult = await _usersController.FindUsersWithClaim(claimName, claimValue);

			// then
			actionResult.ShouldBeOkObjectResult();
			IEnumerable<UserResponse> actualUsers = actionResult.GetOkObjectResultValue();
			actualUsers.Count().ShouldBe(2);
		}

		[Fact]
		public async Task AddAdmin_should_create_user_with_usermanager_and_add_claim()
		{
			// given
			string email = "donald@trump.com";
			string password = "fakepassword";

			_userManagerMock.CreateAsync(
					Arg.Is<RoadkillIdentityUser>(
						u => u.Email == email &&
							 u.EmailConfirmed &&
							 u.UserName == email), password)
				.Returns(Task.FromResult(IdentityResult.Success));

			var requestModel = new UserRequest()
			{
				Email = email,
				Password = password
			};

			// when
			var actionResult = await _usersController.CreateAdmin(requestModel);

			// then
			actionResult.ShouldBeCreatedAtActionResult();
			string actualEmailAddress = actionResult.CreatedAtActionResultValue();
			actualEmailAddress.ShouldBe(email);

			await _userManagerMock
				.Received(1)
				.CreateAsync(
					Arg.Is<RoadkillIdentityUser>(u => u.Email == email), password);

			await _userManagerMock
				.Received(1)
				.AddClaimAsync(
					Arg.Is<RoadkillIdentityUser>(u => u.Email == email),
					Arg.Is<Claim>(c => c.Type == ClaimTypes.Role && c.Value == RoleNames.Admin));
		}

		[Fact]
		public async Task AddEditor_should_create_user_with_usermanager_and_add_claim()
		{
			// given
			string email = "daffy@trump.com";
			string password = "fakepassword";

			_userManagerMock.CreateAsync(
					Arg.Is<RoadkillIdentityUser>(
						u => u.Email == email &&
							 u.EmailConfirmed &&
							 u.UserName == email), password)
				.Returns(Task.FromResult(IdentityResult.Success));

			var requestModel = new UserRequest()
			{
				Email = email,
				Password = password
			};

			// when
			var actionResult = await _usersController.CreateEditor(requestModel);

			// then
			actionResult.ShouldBeCreatedAtActionResult();
			string actualEmailAddress = actionResult.CreatedAtActionResultValue();
			actualEmailAddress.ShouldBe(email);

			await _userManagerMock
				.Received(1)
				.CreateAsync(
					Arg.Is<RoadkillIdentityUser>(u => u.Email == email), password);

			await _userManagerMock
				.Received(1)
				.AddClaimAsync(
					Arg.Is<RoadkillIdentityUser>(u => u.Email == email),
					Arg.Is<Claim>(c => c.Type == ClaimTypes.Role && c.Value == RoleNames.Editor));
		}

		[Fact]
		public async Task AddAdmin_should_return_badrequest_if_email_exists()
		{
			// given
			string email = "donald@trump.com";
			string password = "fakepassword";
			var expectedError = UsersController.EmailExistsError;

			_userManagerMock.FindByEmailAsync(email)
				.Returns(new RoadkillIdentityUser() { Email = email });

			var requestModel = new UserRequest()
			{
				Email = email,
				Password = password
			};

			// when
			var actionResult = await _usersController.CreateAdmin(requestModel);

			// then
			actionResult.ShouldBeBadRequestObjectResult();
			string errorMessage = actionResult.GetBadRequestValue();
			errorMessage.ShouldBe(expectedError);
		}

		[Fact]
		public async Task AddEditor_should_return_badrequest_if_email_exists()
		{
			// given
			string email = "daffy@trump.com";
			string password = "fakepassword";
			var expectedError = UsersController.EmailExistsError;

			_userManagerMock.FindByEmailAsync(email)
				.Returns(new RoadkillIdentityUser() { Email = email });

			var requestModel = new UserRequest()
			{
				Email = email,
				Password = password
			};

			// when
			var actionResult = await _usersController.CreateEditor(requestModel);

			// then
			actionResult.ShouldBeBadRequestObjectResult();
			string errorMessage = actionResult.GetBadRequestValue();
			errorMessage.ShouldBe(expectedError);
		}

		[Fact]
		public async Task DeleteUser_should_return_no_content_and_set_user_as_locked_out()
		{
			// given
			string email = "okfingers@trump.com";

			_userManagerMock.FindByEmailAsync(email)
				.Returns(new RoadkillIdentityUser() { Email = email });

			_userManagerMock.UpdateAsync(Arg.Is<RoadkillIdentityUser>(u => u.Email == email))
				.Returns(Task.FromResult(IdentityResult.Success));

			// when
			var actionResult = await _usersController.DeleteUser(email);

			// then
			actionResult.ShouldBeNoContentResult();

			await _userManagerMock
				.Received(1)
				.UpdateAsync(Arg.Is<RoadkillIdentityUser>(
					u => u.Email == email &&
						 u.LockoutEnabled == true &&
						 u.LockoutEnd == DateTime.MaxValue));
		}

		[Fact]
		public async Task DeleteUser_should_return_notfound_when_user_doesnt_exist()
		{
			// given
			string email = "fakeuser@trump.com";
			var expectedError = UsersController.EmailDoesNotExistError;

			_userManagerMock.FindByEmailAsync(email)
				.Returns(Task.FromResult((RoadkillIdentityUser)null));

			// when
			var actionResult = await _usersController.DeleteUser(email);

			// then
			actionResult.ShouldBeNotFoundObjectResult();
			string errorMessage = actionResult.GetNotFoundValue();
			errorMessage.ShouldBe(expectedError);
		}

		[Fact]
		public async Task DeleteUser_should_return_badrequest_when_user_is_already_lockedout()
		{
			// given
			string email = "lockedout@trump.com";
			var expectedError = UsersController.UserIsLockedOutError;

			_userManagerMock.FindByEmailAsync(email)
				.Returns(new RoadkillIdentityUser()
				{
					Email = email,
					LockoutEnd = DateTime.MaxValue,
					LockoutEnabled = true
				});

			// when
			var actionResult = await _usersController.DeleteUser(email);

			// then
			actionResult.ShouldBeBadRequestObjectResult();
			string errorMessage = actionResult.GetBadRequestValue();
			errorMessage.ShouldBe(expectedError);
		}
	}
}
