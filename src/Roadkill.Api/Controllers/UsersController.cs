using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Roadkill.Api.Common.Models;
using Roadkill.Core.Authorization;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Roadkill.Api.Controllers
{
	[Authorize]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<RoadkillUser> _userManager;

	    public UsersController(UserManager<RoadkillUser> userManager)
	    {
		    _userManager = userManager;
	    }

        [HttpGet]
        [Route(nameof(GetAll))]
        [Authorize(Policy = "Admins")]
        public async Task<IEnumerable<RoadkillUser>> GetAll()
        {
            return await _userManager.Users.ToListAsync();
        }

        [HttpGet]
        [Route(nameof(UsersWithClaim))]
        [Authorize(Policy = "Admins")]
        public async Task<IEnumerable<RoadkillUser>> UsersWithClaim(string claimType, string claimValue)
        {
            return await _userManager.GetUsersForClaimAsync(new Claim(claimType, claimValue));
        }

        [HttpPost]
        [Route(nameof(Add))]
        [Authorize(Policy = "Admins")]
        public async Task<IdentityResult> Add()
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
