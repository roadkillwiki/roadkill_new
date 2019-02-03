using System.Collections.Generic;
using System.Security.Claims;

namespace Roadkill.Api.JWT
{
	public interface IJwtTokenProvider
	{
		string CreateToken(IList<Claim> existingClaims, string email);
	}
}
