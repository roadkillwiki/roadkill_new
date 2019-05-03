using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Models;
using Roadkill.Api.JWT;
using Roadkill.Core.Authorization;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Roadkill.Api.Controllers
{
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
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
		public async Task<ActionResult<string>> Authenticate([FromBody] AuthenticationModel authenticationModel)
		{
			RoadkillUser user = await _userManager.FindByEmailAsync(authenticationModel.Email);
			if (user == null)
			{
				return NotFound($"The user with the email {authenticationModel.Email} could not be found.");
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
