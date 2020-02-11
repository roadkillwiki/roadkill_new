namespace Roadkill.Api.Settings
{
	public class JwtSettings
	{
		public string Password { get; set; }
		public int JwtExpiresMinutes { get; set; }
		public int RefreshTokenExpiresDays { get; set; }
	}
}
