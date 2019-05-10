using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.JWT;
using Roadkill.Core.Authorization;

namespace Roadkill.Api.Controllers
{
	[ApiController]
	[AllowAnonymous]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	public class InstallController : ControllerBase
	{
		private readonly UserManager<RoadkillIdentityUser> _userManager;

		public InstallController(UserManager<RoadkillIdentityUser> userManager)
		{
			_userManager = userManager;
		}

		[HttpPost]
		[Route(nameof(CreateTestUser))]
		public async Task<ActionResult<IdentityResult>> CreateTestUser()
		{
			var newUser = new RoadkillIdentityUser()
			{
				UserName = "admin@localhost",
				Email = "admin@localhost",
				EmailConfirmed = true
			};

			await _userManager.CreateAsync(newUser, "password");
			await _userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Role, RoleNames.Admin));

			return CreatedAtAction(nameof(InstallController.CreateTestUser), newUser.Email);
		}
	}
}
