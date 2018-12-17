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

	    public AuthorizationController(UserManager<RoadkillUser> userManager, SignInManager<RoadkillUser> signInManager)
	    {
		    _userManager = userManager;
		    _signInManager = signInManager;
	    }

        [HttpPost]
        [Route(nameof(Authenticate))]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationModel authenticationModel)
        {
            // chris@example.org/password
            RoadkillUser user = await _userManager.FindByEmailAsync(authenticationModel.Email);

            SignInResult result = await _signInManager.PasswordSignInAsync(user, authenticationModel.Password, true, false);
            if (result.Succeeded)
            {
	            IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);
	            if (userClaims.Count == 0)
	            {
		            return Forbid();
	            }

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user.Email),
                };

	            claims.AddRange(userClaims);

                var key = Encoding.ASCII.GetBytes(DependencyInjection.JwtPassword);
                var symmetricSecurityKey = new SymmetricSecurityKey(key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(7), // TODO: make configurable
                    SigningCredentials =
                        new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                string bearerToken = tokenHandler.WriteToken(token);

                return Ok(bearerToken);
            }

            return Forbid();
        }
    }
}
