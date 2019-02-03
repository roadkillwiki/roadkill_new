using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Roadkill.Api.Controllers;
using Roadkill.Core.Authorization;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	[SuppressMessage("Stylecop", "CA1063", Justification = "IDisposable overkill")]
	[SuppressMessage("Stylecop", "CA1001", Justification = "IDisposable overkill")]
	public sealed class UsersControllerTests
	{
		private readonly Fixture _fixture;
		private UsersController _usersController;
		private UserManager<RoadkillUser> _userManagerMock;

		public UsersControllerTests()
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

			_usersController = new UsersController(_userManagerMock);
		}

		[Fact]
		public async Task GetAll_should_return_all_users_from_manager()
		{
			// given
			var expectedAllUsers = _fixture.CreateMany<RoadkillUser>(5);
			_userManagerMock.Users.Returns(expectedAllUsers.AsQueryable());

			foreach (RoadkillUser user in expectedAllUsers)
			{
				await _mockUserStore.CreateAsync(user);
			}

			// when
			IEnumerable<RoadkillUser> actualAllUsers = await _usersController.GetAll();

			// then
			actualAllUsers.Count().ShouldBe(expectedAllUsers.Count());
		}
	}
}
