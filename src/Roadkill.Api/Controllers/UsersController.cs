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

        private readonly SignInManager<RoadkillUser> _signInManager;

	    public UsersController(UserManager<RoadkillUser> userManager, SignInManager<RoadkillUser> signInManager)
	    {
		    _userManager = userManager;
		    _signInManager = signInManager;
	    }

        [HttpGet]
        [Route(nameof(GetAll))]
        [Authorize(Policy = "Admins")]
        public async Task<IEnumerable<RoadkillUser>> GetAll()
        {
            return await _userManager.Users.ToListAsync();
        }

        [HttpPost]
        [Route(nameof(Authenticate))]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationModel authenticationModel)
        {
            // chris@example.org/password
            RoadkillUser user = await _userManager.FindByEmailAsync(authenticationModel.Email);
	        if (user == null)
	        {
		        await AddTestUsers();
	        }

            user = await _userManager.FindByEmailAsync(authenticationModel.Email);

            SignInResult result = await _signInManager.PasswordSignInAsync(user, authenticationModel.Password, true, false);
            if (result.Succeeded)
            {
                Claim roleClaim = null;
                if (authenticationModel.Email == "editor@example.org")
                {
                    roleClaim = new Claim(ClaimTypes.Role, "Editor");
                }
                else if (authenticationModel.Email == "admin@example.org")
                {
                    roleClaim = new Claim(ClaimTypes.Role, "Admin");
                }

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    roleClaim
                };

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
