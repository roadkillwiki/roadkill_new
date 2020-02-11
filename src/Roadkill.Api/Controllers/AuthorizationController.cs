using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Authorization.JWT;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
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
					return StatusCode(StatusCodes.Status403Forbidden);
				}

				string refreshToken = Guid.NewGuid().ToString("N");
				string email = identityUser.Email;

				AuthorizationResponse response = await GetAuthorizationResponse(refreshToken, email, existingClaims.ToList());
				return Ok(response);
			}

			// don't use Forbid() as that goes through ASP.NET Core's authentication middleware, e.g. cookies
			return StatusCode(StatusCodes.Status403Forbidden);
		}

		[HttpPost]
		[Route(nameof(RefreshToken))]
		[AllowAnonymous]
		public async Task<ActionResult<AuthorizationResponse>> RefreshToken([FromBody] string existingRefreshToken)
		{
			UserRefreshToken userRefreshToken = await _jwtTokenService.GetExistingRefreshToken(existingRefreshToken);
			if (userRefreshToken != null)
			{
				JwtSecurityToken jwtSecurityToken = _jwtTokenService.GetJwtSecurityToken(userRefreshToken.JwtToken);
				List<Claim> existingClaims = jwtSecurityToken.Claims.ToList();
				string email = existingClaims.First(x => x.Type == ClaimTypes.Name).Value;

				string refreshToken = userRefreshToken.RefreshToken;
				AuthorizationResponse response = await GetAuthorizationResponse(refreshToken, email, existingClaims);
				return Ok(response);
			}

			return StatusCode(StatusCodes.Status404NotFound);
		}

		private async Task<AuthorizationResponse> GetAuthorizationResponse(string refreshToken, string email, List<Claim> existingClaims)
		{
			string ip = GetIpAddress();
			string jwtToken = _jwtTokenService.CreateJwtToken(existingClaims, email);
			string newRefreshToken = await _jwtTokenService.StoreRefreshToken(jwtToken, refreshToken, ip, email);

			var response = new AuthorizationResponse()
			{
				JwtToken = jwtToken,
				RefreshToken = newRefreshToken
			};

			return response;
		}

		private string GetIpAddress()
		{
			// When testing on localhost, RemoteIpAddress is null.
			var ipAddress = HttpContext.Connection.RemoteIpAddress ?? IPAddress.Loopback;
			return ipAddress.ToString();
		}
	}
}
