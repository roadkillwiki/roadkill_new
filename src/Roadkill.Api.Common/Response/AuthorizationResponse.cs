namespace Roadkill.Api.Common.Response
{
	public class AuthorizationResponse
	{
		public string JwtToken { get; set; }
		public string RefreshToken { get; set; }
	}
}
