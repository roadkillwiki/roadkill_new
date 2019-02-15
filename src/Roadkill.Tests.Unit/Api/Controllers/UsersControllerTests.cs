using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NSubstitute;
using Roadkill.Api.Controllers;
using Roadkill.Api.JWT;
using Roadkill.Core.Authorization;
using Roadkill.Tests.Unit.Api.JWT;
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

			// when
			IEnumerable<RoadkillUser> actualAllUsers = await _usersController.GetAll();

			// then
			actualAllUsers.Count().ShouldBe(expectedAllUsers.Count());
		}

		[Fact]
		public async Task FindUsersWithClaim_should_return_specific_users_for_claim()
		{
			// given
			string claimName = ClaimTypes.Role;
			string claimValue = RoleNames.Admin;

			var expectedUsers = new List<RoadkillUser>()
			{
				_fixture.Create<RoadkillUser>(),
				_fixture.Create<RoadkillUser>()
			};

			_userManagerMock.GetUsersForClaimAsync(Arg.Is<Claim>(c => c.Type == claimName && c.Value == claimValue))
				.Returns(Task.FromResult((IList<RoadkillUser>)expectedUsers));

			// when
			IEnumerable<RoadkillUser> actualUsers =
				await _usersController.FindUsersWithClaim(claimName, claimValue);

			// then
			actualUsers.ShouldNotBeNull();
			actualUsers.ShouldBe(expectedUsers);
		}
	}
}
