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
    [Route("[controller]")]
    public class InstallController : ControllerBase
    {
        private readonly UserManager<RoadkillUser> _userManager;

	    public InstallController(UserManager<RoadkillUser> userManager)
	    {
		    _userManager = userManager;
	    }

        [HttpPost]
        [Route(nameof(CreateTestUser))]
        public async Task<ActionResult<IdentityResult>> CreateTestUser()
        {
	        var newUser = new RoadkillUser()
	        {
		        UserName = "admin@localhost",
		        Email = "admin@localhost",
		        EmailConfirmed = true
	        };

	        await _userManager.CreateAsync(newUser, "password");
	        await _userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Role, RoleNames.Admin));

	        return CreatedAtRoute(nameof(UsersController.GetByEmail), newUser.Email);
        }
    }
}
