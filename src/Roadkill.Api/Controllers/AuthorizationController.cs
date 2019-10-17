using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.JWT;
using Roadkill.Core.Entities.Authorization;
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
		private readonly IJwtTokenService _jwtTokenService;

		public AuthorizationController(
			UserManager<RoadkillIdentityUser> userManager,
			SignInManager<RoadkillIdentityUser> signInManager,
			IJwtTokenService jwtTokenService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_jwtTokenService = jwtTokenService;
		}

		[HttpPost]
		[Route(nameof(Authenticate))]
		[AllowAnonymous]
		public async Task<ActionResult<AuthorizationResponse>> Authenticate([FromBody] AuthorizationRequest authorizationRequest)
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

				// When testing on localhost, RemoteIpAddress is null.
				var ipAddress = HttpContext.Connection.RemoteIpAddress ?? IPAddress.Loopback;
				string ip = ipAddress.ToString();
				var token = new AuthorizationResponse()
				{
					JwtToken = _jwtTokenService.CreateToken(existingClaims, identityUser.Email),
					RefreshToken = await _jwtTokenService.CreateRefreshToken(identityUser.Email, ip)
				};

				return Ok(token);
			}

			return Forbid();
		}
	}
}
