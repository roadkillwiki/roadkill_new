using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Roadkill.Api.Settings;
using Roadkill.Core.Entities.Authorization;
using Roadkill.Core.Repositories;

namespace Roadkill.Api.Authorization.JWT
{
	public interface IJwtTokenService
	{
		string CreateToken(IList<Claim> existingClaims, string email);
		Task<string> CreateRefreshToken(string email, string ipAddress);
	}

	public class JwtTokenService : IJwtTokenService
	{
		private readonly JwtSettings _jwtSettings;
		private readonly SecurityTokenHandler _tokenHandler;
		private readonly IUserRefreshTokenRepository _refreshTokenRepository;

		public JwtTokenService(
			JwtSettings jwtSettings,
			SecurityTokenHandler tokenHandler,
			IUserRefreshTokenRepository refreshTokenRepository)
		{
			_jwtSettings = jwtSettings;
			_tokenHandler = tokenHandler;
			_refreshTokenRepository = refreshTokenRepository;
		}

		public string CreateToken(IList<Claim> existingClaims, string email)
		{
			var allClaims = new List<Claim>(existingClaims)
			{
				new Claim(ClaimTypes.Name, email)
			};

			var key = Encoding.ASCII.GetBytes(_jwtSettings.Password);
			var symmetricSecurityKey = new SymmetricSecurityKey(key);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(allClaims),
				Expires = DateTime.UtcNow.AddDays(_jwtSettings.ExpiresDays),
				SigningCredentials =
					new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
			};

			SecurityToken token = _tokenHandler.CreateToken(tokenDescriptor);
			return _tokenHandler.WriteToken(token);
		}

		public async Task<string> CreateRefreshToken(string email, string ipAddress)
		{
			string refreshToken = Guid.NewGuid().ToString("N");
			UserRefreshToken userRefreshToken = await _refreshTokenRepository.AddRefreshToken(email, refreshToken, ipAddress);

			return userRefreshToken.RefreshToken;
		}
	}
}
