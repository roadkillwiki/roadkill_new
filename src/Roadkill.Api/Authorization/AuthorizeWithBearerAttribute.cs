using Microsoft.AspNetCore.Authorization;

namespace Roadkill.Api.Authorization
{
	public class AuthorizeWithBearerAttribute : AuthorizeAttribute
	{
		public AuthorizeWithBearerAttribute()
		{
			AuthenticationSchemes = "Bearer";
		}
	}
}
