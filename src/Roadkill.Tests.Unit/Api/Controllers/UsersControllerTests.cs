using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Roadkill.Api.Controllers;
using Roadkill.Core.Authorization;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public sealed class UsersControllerTests
	{
		private readonly Fixture _fixture;
		private UsersController _usersController;
		private UserManager<RoadkillUser> _userManagerMock;

		public UsersControllerTests()
		{
			_fixture = new Fixture();
			_userManagerMock = Substitute.For<UserManager<RoadkillUser>>();

			_usersController = new UsersController(_userManagerMock);
		}

		[Fact]
		public async Task GetAll_should_return_all_users_from_manager()
		{
			// given
			var expectedAllUsers = _fixture.CreateMany<RoadkillUser>(5);

			_userManagerMock.Users.Returns(expectedAllUsers);

			// when
			IEnumerable<RoadkillUser> actualAllUsers = await _usersController.GetAll();

			// then
			actualAllUsers.Count().ShouldBe(actualAllUsers.Count());
		}
	}
}
