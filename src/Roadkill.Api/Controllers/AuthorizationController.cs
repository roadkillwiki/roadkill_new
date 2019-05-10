using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Request;
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
		private readonly UserManager<RoadkillIdentityUser> _userManager;
		private readonly SignInManager<RoadkillIdentityUser> _signInManager;
		private readonly IJwtTokenProvider _jwtTokenProvider;

		public AuthorizationController(
			UserManager<RoadkillIdentityUser> userManager,
			SignInManager<RoadkillIdentityUser> signInManager,
			IJwtTokenProvider jwtTokenProvider)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_jwtTokenProvider = jwtTokenProvider;
		}

		[HttpPost]
		[Route(nameof(Authenticate))]
		[AllowAnonymous]
		public async Task<ActionResult<string>> Authenticate([FromBody] AuthorizationRequest authorizationRequest)
		{
			RoadkillIdentityUser identityUser = await _userManager.FindByEmailAsync(authorizationRequest.Email);
			if (identityUser == null)
			{
				return NotFound($"The user with the email {authorizationRequest.Email} could not be found.");
			}

			SignInResult result = await _signInManager.PasswordSignInAsync(identityUser, authorizationRequest.Password, true, false);
			if (result.Succeeded)
			{
				IList<Claim> existingClaims = await _userManager.GetClaimsAsync(identityUser);
				if (existingClaims.Count == 0)
				{
					return Forbid();
				}

				string token = _jwtTokenProvider.CreateToken(existingClaims, identityUser.Email);

				return Ok(token);
			}

			return Forbid();
		}
	}
}
