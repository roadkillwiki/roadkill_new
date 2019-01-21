using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Roadkill.Core.Authorization;

namespace Roadkill.Tests.Unit.Mocks
{
	// From https://github.com/aspnet/Identity/blob/master/test/Shared/MockHelpers.cs
	public static class MockIdentityManagersFactory
	{
		public static StringBuilder LogMessage { get; set; } = new StringBuilder();

		public static Mock<UserManager<TUser>> MockUserManager<TUser>()
	        where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
            return mgr;
        }

        public static Mock<RoleManager<TRole>> MockRoleManager<TRole>(IRoleStore<TRole> store = null)
	        where TRole : class
        {
            store = store ?? new Mock<IRoleStore<TRole>>().Object;
            var roles = new List<IRoleValidator<TRole>>();
            roles.Add(new RoleValidator<TRole>());

            return new Mock<RoleManager<TRole>>(
	            store,
	            roles,
	            new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
	            null);
        }

        public static UserManager<TUser> TestUserManager<TUser>(IUserStore<TUser> store = null)
	        where TUser : class
        {
            store = store ?? new Mock<IUserStore<TUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;
            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<TUser>>();
            var validator = new Mock<IUserValidator<TUser>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<TUser>>();
            pwdValidators.Add(new PasswordValidator<TUser>());

            var userManager = new UserManager<TUser>(
	            store,
	            options.Object,
	            new PasswordHasher<TUser>(),
                userValidators,
                pwdValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
	            null,
                new Mock<ILogger<UserManager<TUser>>>().Object);

            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
            return userManager;
        }

        public static RoleManager<TRole> TestRoleManager<TRole>(IRoleStore<TRole> store = null)
	        where TRole : class
        {
            store = store ?? new Mock<IRoleStore<TRole>>().Object;
            var roles = new List<IRoleValidator<TRole>>();
            roles.Add(new RoleValidator<TRole>());

            return new RoleManager<TRole>(
	            store,
	            roles,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null);
        }

		public static UserManager<RoadkillUser> CreateUserManager(InMemoryUserStore<RoadkillUser> userStore = null)
		{
			if (userStore == null)
            {
                userStore = new InMemoryUserStore<RoadkillUser>();
            }

            return new UserManager<RoadkillUser>(
				userStore,
				null,
				new PasswordHasher<RoadkillUser>(),
				null,
				null,
				null,
				null,
				new ServiceCollection().BuildServiceProvider(),
				new NullLogger<UserManager<RoadkillUser>>());
		}

		public static SignInManager<RoadkillUser> CreateSigninManager(UserManager<RoadkillUser> userManager)
		{
			var contextAccessor = new Mock<IHttpContextAccessor>();
			contextAccessor.Setup(x => x.HttpContext).Returns(new Mock<HttpContext>().Object);

			return new SignInManager<RoadkillUser>(
				userManager,
				contextAccessor.Object,
				new Mock<IUserClaimsPrincipalFactory<RoadkillUser>>().Object,
				null,
				new NullLogger<SignInManager<RoadkillUser>>(),
				null);
		}
	}
}
