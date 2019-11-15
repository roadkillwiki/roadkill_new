using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Authorization;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.Exceptions;
using Roadkill.Api.ObjectConverters;
using Roadkill.Core.Entities.Authorization;

namespace Roadkill.Api.Controllers
{
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	[Authorize]
	public class UsersController : ControllerBase
	{
		public static readonly string EmailExistsError = "The email address already exists.";
		public static readonly string EmailDoesNotExistError = "The email address does not exist.";
		public static readonly string UserIsLockedOutError = "The user with the email address is already locked out.";
		private readonly UserManager<RoadkillIdentityUser> _userManager;
		private readonly IUserObjectsConverter _userObjectsConverter;

		public UsersController(
			UserManager<RoadkillIdentityUser> userManager,
			IUserObjectsConverter userObjectsConverter)
		{
			_userManager = userManager;
			_userObjectsConverter = userObjectsConverter;
		}

		[HttpGet]
		[Route("{email}")]
		[Authorize(Policy = PolicyNames.GetUser)]
		public async Task<ActionResult<UserResponse>> Get(string email)
		{
			RoadkillIdentityUser identityUser = await _userManager.FindByEmailAsync(email);
			if (identityUser == null)
			{
				return NotFound(EmailDoesNotExistError);
			}

			UserResponse response = _userObjectsConverter.ConvertToUserResponse(identityUser);

			return Ok(response);
		}

		[HttpGet]
		[Route(nameof(FindAll))]
		[Authorize(Policy = PolicyNames.FindUsers)]
		public ActionResult<IEnumerable<UserResponse>> FindAll()
		{
			IEnumerable<RoadkillIdentityUser> roadkillUsers = _userManager.Users.ToList();
			IEnumerable<UserResponse> responses =
				roadkillUsers.Select(u => _userObjectsConverter.ConvertToUserResponse(u));

			return Ok(responses);
		}

		[HttpGet]
		[Route(nameof(FindUsersWithClaim))]
		[Authorize(Policy = PolicyNames.FindUsers)]
		public async Task<ActionResult<IEnumerable<UserResponse>>> FindUsersWithClaim(string claimType, string claimValue)
		{
			var claim = new Claim(claimType, claimValue);
			IList<RoadkillIdentityUser> usersForClaim = await _userManager.GetUsersForClaimAsync(claim);

			IEnumerable<UserResponse> responses =
				usersForClaim.Select(u => _userObjectsConverter.ConvertToUserResponse(u));

			return Ok(responses);
		}

		[HttpPost]
		[Route(nameof(CreateAdmin))]
		[Authorize(Policy = PolicyNames.CreateAdminUser)]
		public async Task<ActionResult<string>> CreateAdmin(UserRequest userRequest)
		{
			RoadkillIdentityUser user = await _userManager.FindByEmailAsync(userRequest.Email);
			if (user != null)
			{
				return BadRequest(EmailExistsError);
			}

			var newUser = new RoadkillIdentityUser()
			{
				UserName = userRequest.Email,
				Email = userRequest.Email,
				EmailConfirmed = true
			};

			IdentityResult result = await _userManager.CreateAsync(newUser, userRequest.Password);
			if (!result.Succeeded)
			{
				throw new ApiException($"Unable to create admin user {userRequest.Email} - UserManager call failed." + string.Join("\n", result.Errors));
			}

			await _userManager.AddClaimAsync(newUser, RoadkillClaims.AdminClaim);

			return CreatedAtAction(nameof(CreateAdmin), userRequest.Email);
		}

		[HttpPost]
		[Route(nameof(CreateEditor))]
		[Authorize(Policy = PolicyNames.CreateEditorUser)]
		public async Task<ActionResult<string>> CreateEditor(UserRequest userRequest)
		{
			RoadkillIdentityUser user = await _userManager.FindByEmailAsync(userRequest.Email);
			if (user != null)
			{
				return BadRequest(EmailExistsError);
			}

			var newUser = new RoadkillIdentityUser()
			{
				UserName = userRequest.Email,
				Email = userRequest.Email,
				EmailConfirmed = true
			};

			IdentityResult result = await _userManager.CreateAsync(newUser, userRequest.Password);
			if (!result.Succeeded)
			{
				throw new ApiException($"Unable to create editor user {userRequest.Email} - UserManager call failed." + string.Join("\n", result.Errors));
			}

			await _userManager.AddClaimAsync(newUser, RoadkillClaims.EditorClaim);

			return CreatedAtAction(nameof(CreateEditor), userRequest.Email);
		}

		[HttpDelete]
		[Route(nameof(Delete))]
		[Authorize(Policy = PolicyNames.DeleteUser)]
		public async Task<ActionResult<string>> Delete([FromBody]string email)
		{
			RoadkillIdentityUser identityUser = await _userManager.FindByEmailAsync(email);
			if (identityUser == null)
			{
				return NotFound(EmailDoesNotExistError);
			}

			if (identityUser.LockoutEnabled)
			{
				return BadRequest(UserIsLockedOutError);
			}

			identityUser.LockoutEnd = DateTime.MaxValue;
			identityUser.LockoutEnabled = true;

			IdentityResult result = await _userManager.UpdateAsync(identityUser);
			if (!result.Succeeded)
			{
				throw new ApiException($"Unable to delete user {email} - UserManager call failed." + string.Join("\n", result.Errors));
			}

			return NoContent();
		}
	}
}
