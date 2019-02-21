using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Marten;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Roadkill.Api.Common.Models;
using Roadkill.Api.JWT;
using Roadkill.Api.Settings;
using Roadkill.Core.Authorization;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Roadkill.Api.Controllers
{
	[Authorize]
    [Route("[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly UserManager<RoadkillUser> _userManager;

        private readonly SignInManager<RoadkillUser> _signInManager;
        private readonly IJwtTokenProvider _jwtTokenProvider;

        public AuthorizationController(
		    UserManager<RoadkillUser> userManager,
		    SignInManager<RoadkillUser> signInManager,
		    IJwtTokenProvider jwtTokenProvider)
	    {
		    _userManager = userManager;
		    _signInManager = signInManager;
		    _jwtTokenProvider = jwtTokenProvider;
	    }

        [HttpPost]
        [Route(nameof(Authenticate))]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationModel authenticationModel)
        {
            RoadkillUser user = await _userManager.FindByEmailAsync(authenticationModel.Email);
            if (user == null)
            {
	            return Forbid();
            }

            if (user.LockoutEnabled)
            {
	            // Lockout is used to delete users as well as lock them out
	            return Forbid();
            }

            SignInResult result = await _signInManager.PasswordSignInAsync(user, authenticationModel.Password, true, false);
            if (result.Succeeded)
            {
	            IList<Claim> existingClaims = await _userManager.GetClaimsAsync(user);
	            if (existingClaims.Count == 0)
	            {
		            return Forbid();
	            }

	            string token = _jwtTokenProvider.CreateToken(existingClaims, user.Email);

                return Ok(token);
            }

            return Forbid();
        }
    }
}
