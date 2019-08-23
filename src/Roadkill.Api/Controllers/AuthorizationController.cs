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
				return NotFound($"The user with the email '{authorizationRequest.Email}' could not be found.");
			}

			SignInResult result = await _signInManager.PasswordSignInAsync(identityUser, authorizationRequest.Password, true, false);
			if (result.Succeeded)
			{
				IList<Claim> existingClaims = await _userManager.GetClaimsAsync(identityUser);
				if (existingClaims.Count == 0)
				{
					return Forbid();
				}

				string ip = GetIpAddress();
				var response = new AuthorizationResponse()
				{
					JwtToken = _jwtTokenService.CreateToken(existingClaims, identityUser.Email),
					RefreshToken = await _jwtTokenService.CreateRefreshToken(identityUser.Email, ip)
				};

				return Ok(response);
			}

			return Forbid();
		}

		[HttpPost]
		[Route(nameof(RefreshToken))]
		[AllowAnonymous]
		public async Task<ActionResult<AuthorizationResponse>> RefreshToken(string refreshToken)
		{
			// A short explanation on refresh tokens:
			//
			// - JWT tokens are always short-lived (ideally < 5 minutes)
			// - The refresh token allows clients to skip sending auth credentials (email/password) when their JWT token expires.
			// - It's not part of the JWT token itself, but an additional value returned as part of AuthorizationResponse.
			// - We return a new JWT token when RefreshToken() is called.
			// - The refresh token can be long lived, as it's stored server side inside Postgres,
			// - The refresh token is locked to one device (IP address) for a user.
			// - Although long-lived, the refresh token are renewed by this method.
			// - The JWT token is also renew/recreated by this method - likely every 5 minutes.
			// - ASP.NET Core's "services.AddAuthorization()" handles validation of JWT token expiries.

			string ip = GetIpAddress();
			string email = await _jwtTokenService.GetEmailByRefreshToken(refreshToken, ip);
			if (string.IsNullOrEmpty(email))
			{
				return NotFound($"The refresh token does not exist, or may have expired.");
			}

			RoadkillIdentityUser identityUser = await _userManager.FindByEmailAsync(email);
			if (identityUser == null)
			{
				return NotFound($"The refresh token '{refreshToken}' was found, but the user with the email '{email}' could not be found.");
			}

			IList<Claim> existingClaims = await _userManager.GetClaimsAsync(identityUser);
			if (existingClaims.Count == 0)
			{
				return Forbid();
			}

			var response = new AuthorizationResponse()
			{
				JwtToken = _jwtTokenService.CreateToken(existingClaims, identityUser.Email),
				RefreshToken = await _jwtTokenService.CreateRefreshToken(identityUser.Email, ip)
			};

			return Ok(response);
		}

		private string GetIpAddress()
		{
			// When testing on localhost, RemoteIpAddress is null.
			var ipAddress = HttpContext.Connection.RemoteIpAddress ?? IPAddress.Loopback;
			return ipAddress.ToString();
		}
	}
}
