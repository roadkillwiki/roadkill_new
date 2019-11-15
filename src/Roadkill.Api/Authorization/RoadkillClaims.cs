using System.Security.Claims;

namespace Roadkill.Api.Authorization
{
	public static class RoadkillClaims
	{
		public static Claim AdminClaim => new Claim(ClaimTypes.Role, AdminRoleDefinition.Name);
		public static Claim EditorClaim => new Claim(ClaimTypes.Role, EditorRoleDefinition.Name);
	}
}
