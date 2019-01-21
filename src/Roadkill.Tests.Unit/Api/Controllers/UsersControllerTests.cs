using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Marten;
using Marten.AspNetIdentity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.InMemory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using NSubstitute.Extensions;
using Roadkill.Api.Controllers;
using Roadkill.Core.Authorization;
using Roadkill.Tests.Unit.Mocks;
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
		private InMemoryUserStore<RoadkillUser> _mockUserStore;

		public UsersControllerTests()
		{
			_fixture = new Fixture();
			_mockUserStore = new InMemoryUserStore<RoadkillUser>();
			_userManagerMock = MockIdentityManagersFactory.CreateUserManager(_mockUserStore);

			_usersController = new UsersController(_userManagerMock);
		}

		[Fact]
		public async Task GetAll_should_return_all_users_from_manager()
		{
			// given
			var expectedAllUsers = _fixture.CreateMany<RoadkillUser>(5);

			foreach (RoadkillUser user in expectedAllUsers)
			{
				await _mockUserStore.CreateAsync(user);
			}

			// when
			IEnumerable<RoadkillUser> actualAllUsers = await _usersController.GetAll();

			// then
			actualAllUsers.Count().ShouldBe(expectedAllUsers.Count());
		}

		[Fact]
		public async Task PasswordSignInReturnsLockedOutWhenLockedOut()
		{
			// Setup
			var user = new RoadkillUser() { UserName = "Foo" };
			var manager = MockIdentityManagersFactory.MockUserManager<RoadkillUser>();
			manager.Setup(m => m.SupportsUserLockout).Returns(true).Verifiable();
			manager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true).Verifiable();

			var context = new Mock<HttpContext>();
			var contextAccessor = new Mock<IHttpContextAccessor>();
			contextAccessor.Setup(a => a.HttpContext).Returns(context.Object);
			var roleManager = MockIdentityManagersFactory.MockRoleManager<MockRole>();
			var identityOptions = new IdentityOptions();
			var options = new Mock<IOptions<IdentityOptions>>();
			options.Setup(a => a.Value).Returns(identityOptions);
			var claimsFactory = new UserClaimsPrincipalFactory<RoadkillUser, MockRole>(manager.Object, roleManager.Object, options.Object);
			var logger = new NullLogger<SignInManager<RoadkillUser>>();
			var helper = new SignInManager<RoadkillUser>(manager.Object, contextAccessor.Object, claimsFactory, options.Object, logger, new Mock<IAuthenticationSchemeProvider>().Object);

			// Act
			var result = await helper.PasswordSignInAsync(user.UserName, "bogus", false, false);

			// Assert
			Assert.False(result.Succeeded);
			Assert.True(result.IsLockedOut);
			manager.Verify();
		}
	}
}
