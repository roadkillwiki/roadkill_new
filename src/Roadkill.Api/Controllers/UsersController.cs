using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Roadkill.Api.Common.Models;
using Roadkill.Api.JWT;
using Roadkill.Core.Authorization;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Roadkill.Api.Controllers
{
	[Authorize]
    [Route("[controller]")]
	[Authorize(Policy = PolicyNames.Admin)]
    public class UsersController : ControllerBase
    {
	    public static readonly IdentityError EmailExistsError = new IdentityError()
	    {
		    Code = "EmailAlreadyExists",
		    Description = "The email address already exists."
	    };

	    public static readonly IdentityError EmailDoesNotExistError = new IdentityError()
	    {
		    Code = "EmailDoesNotExist",
		    Description = "The email address does not exist."
	    };

	    public static readonly IdentityError UserIsLockedOut = new IdentityError()
	    {
		    Code = "UserAlreadyLockedOut",
		    Description = "The user with the email address is already locked out."
	    };

        private readonly UserManager<RoadkillUser> _userManager;

	    public UsersController(UserManager<RoadkillUser> userManager)
	    {
		    _userManager = userManager;
	    }

        [HttpGet]
        [Route(nameof(GetAll))]
        public Task<IEnumerable<RoadkillUser>> GetAll()
        {
	        return Task.FromResult(_userManager.Users.AsEnumerable());
        }

        [HttpGet]
        [Route(nameof(FindUsersWithClaim))]
        public async Task<IEnumerable<RoadkillUser>> FindUsersWithClaim(string claimType, string claimValue)
        {
            return await _userManager.GetUsersForClaimAsync(new Claim(claimType, claimValue));
        }

        [HttpPost]
        [Route(nameof(AddAdmin))]
        public async Task<IdentityResult> AddAdmin(string email, string password)
        {
	        var user = await _userManager.FindByEmailAsync(email);
	        if (user != null)
	        {
		        return IdentityResult.Failed(EmailExistsError);
	        }

            var newUser = new RoadkillUser()
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            IdentityResult result = await _userManager.CreateAsync(newUser, password);
            await _userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Role, RoleNames.Admin));

            return result;
        }

        [HttpPost]
        [Route(nameof(AddEditor))]
        public async Task<IdentityResult> AddEditor(string email, string password)
        {
	        var user = await _userManager.FindByEmailAsync(email);
	        if (user != null)
	        {
		        return IdentityResult.Failed(EmailExistsError);
	        }

	        var newUser = new RoadkillUser()
	        {
		        UserName = email,
		        Email = email,
		        EmailConfirmed = true
	        };

	        IdentityResult result = await _userManager.CreateAsync(newUser, password);
	        await _userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Role, RoleNames.Editor));

	        return result;
        }

        [HttpPost]
        [Route(nameof(DeleteUser))]
        public async Task<IdentityResult> DeleteUser(string email)
        {
	        var user = await _userManager.FindByEmailAsync(email);
	        if (user == null)
	        {
		        return IdentityResult.Failed(EmailExistsError);
	        }

	        if (user.LockoutEnabled)
	        {
		        return IdentityResult.Failed(UserIsLockedOut);
	        }

	        user.LockoutEnd = DateTime.MaxValue;
	        user.LockoutEnabled = true;

	        IdentityResult result = await _userManager.UpdateAsync(user);
	        return result;
        }

        [HttpPost]
        [Route(nameof(AddTestUser))]
        public async Task<IdentityResult> AddTestUser()
        {
	        var newUser = new RoadkillUser()
	        {
		        UserName = "chris@example.org",
		        Email = "chris@example.org",
		        EmailConfirmed = true
	        };

	        var user = await _userManager.FindByEmailAsync("chris@example.org");
	        await _userManager.DeleteAsync(newUser);

	        var result = await _userManager.CreateAsync(newUser, "password");

	        await _userManager.AddClaimAsync(newUser, new Claim("ApiUser", "CanAddPage"));

	        return result;
        }

	    private async Task AddTestUsers()
	    {
		    var editorUser = new RoadkillUser()
		    {
			    UserName = "editor@example.org",
			    Email = "editor@example.org",
		    };
		    await _userManager.CreateAsync(editorUser, "password");

		    var adminUser = new RoadkillUser()
		    {
			    UserName = "admin@example.org",
			    Email = "admin@example.org",
		    };
		    await _userManager.CreateAsync(adminUser, "password");
	    }
    }
}
