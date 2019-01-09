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
        private readonly JwtSettings _jwtSettings;

        public AuthorizationController(
		    UserManager<RoadkillUser> userManager,
		    SignInManager<RoadkillUser> signInManager,
		    JwtSettings jwtSettings)
	    {
		    _userManager = userManager;
		    _signInManager = signInManager;
		    _jwtSettings = jwtSettings;
	    }

        [HttpPost]
        [Route(nameof(Authenticate))]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationModel authenticationModel)
        {
            RoadkillUser user = await _userManager.FindByEmailAsync(authenticationModel.Email);

            SignInResult result = await _signInManager.PasswordSignInAsync(user, authenticationModel.Password, true, false);
            if (result.Succeeded)
            {
	            IList<Claim> existingClaims = await _userManager.GetClaimsAsync(user);
	            if (existingClaims.Count == 0)
	            {
		            return Forbid();
	            }

	            var allClaims = new List<Claim>(existingClaims)
	            {
		            new Claim(ClaimTypes.Name, user.Email)
	            };

	            var key = Encoding.ASCII.GetBytes(_jwtSettings.Password);
                var symmetricSecurityKey = new SymmetricSecurityKey(key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(allClaims),
                    Expires = DateTime.UtcNow.AddDays(_jwtSettings.ExpireDays),
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
