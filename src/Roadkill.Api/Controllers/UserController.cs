using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Core.Models;

namespace Roadkill.Api.Controllers
{
	[Route("users")]
	public class UserController : Controller//, IUserService
	{
		private readonly UserManager<RoadkillUser> _userManager;
		private readonly SignInManager<RoadkillUser> _signInManager;

		public UserController(UserManager<RoadkillUser> userManager, SignInManager<RoadkillUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[HttpGet]
		[Route(nameof(GetAll))]
		public async Task<IEnumerable<RoadkillUser>> GetAll()
		{
			return _userManager.Users.ToList();
		}

		[HttpPost]
		[Route(nameof(SignIn))]
		public async Task<Microsoft.AspNetCore.Identity.SignInResult> SignIn(string email, string password)
		{
			var user = await _userManager.FindByEmailAsync("chris@example.org");
			return await _signInManager.PasswordSignInAsync(user, "password", true, false);
		}

		[HttpGet]
		[Route(nameof(UsersWithClaim))]
		public async Task<IEnumerable<RoadkillUser>> UsersWithClaim(string claimType, string claimValue)
		{
			return await _userManager.GetUsersForClaimAsync(new Claim(claimType, claimValue));
		}

		[HttpPost]
		[Route(nameof(Add))]
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
	}
}