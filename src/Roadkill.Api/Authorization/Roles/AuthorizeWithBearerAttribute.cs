using Microsoft.AspNetCore.Authorization;

namespace Roadkill.Api.Authorization.Roles
{
	public class AuthorizeWithBearerAttribute : AuthorizeAttribute
	{
		public AuthorizeWithBearerAttribute()
		{
			AuthenticationSchemes = "Bearer";
		}
	}
}
