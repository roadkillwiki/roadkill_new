using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
		Task<UserRefreshToken> GetExistingRefreshToken(string existingRefreshToken);
		JwtSecurityToken GetJwtSecurityToken(string jwtToken);
		Task<string> StoreRefreshToken(string jwtToken, string refreshToken, string email, string ipAddress);
		string CreateJwtToken(IList<Claim> existingClaims, string email);
	}

	public class JwtTokenService : IJwtTokenService
	{
		private readonly JwtSettings _jwtSettings;
		private readonly SecurityTokenHandler _tokenHandler;
		private readonly IUserRefreshTokenRepository _refreshTokenRepository;
		private readonly TokenValidationParameters _tokenValidationParameters;

		public JwtTokenService(
			JwtSettings jwtSettings,
			SecurityTokenHandler tokenHandler,
			IUserRefreshTokenRepository refreshTokenRepository,
			TokenValidationParameters tokenValidationParameters)
		{
			_jwtSettings = jwtSettings;
			_tokenHandler = tokenHandler;
			_refreshTokenRepository = refreshTokenRepository;
			_tokenValidationParameters = tokenValidationParameters;
		}

		public async Task<UserRefreshToken> GetExistingRefreshToken(string existingRefreshToken)
		{
			if (string.IsNullOrEmpty(existingRefreshToken))
				return null;

			UserRefreshToken userRefreshToken = await _refreshTokenRepository.GetRefreshToken(existingRefreshToken);
			if (userRefreshToken != null)
			{
				var expireDate = userRefreshToken.CreationDate.AddDays(_jwtSettings.RefreshTokenExpiresDays);
				if (expireDate > DateTime.UtcNow)
				{
					try
					{
						ClaimsPrincipal principal = _tokenHandler.ValidateToken(userRefreshToken.JwtToken, _tokenValidationParameters, out SecurityToken securityToken);

						if (principal != null && securityToken != null)
							return userRefreshToken;
					}
					catch (Exception e)
					{
						return null;
					}
				}
			}

			return null;
		}

		public JwtSecurityToken GetJwtSecurityToken(string jwtToken)
		{
			try
			{
				ClaimsPrincipal principal = _tokenHandler.ValidateToken(jwtToken, _tokenValidationParameters, out SecurityToken securityToken);
				return securityToken as JwtSecurityToken;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		public async Task<string> StoreRefreshToken(string jwtToken, string refreshToken, string email, string ipAddress)
		{
			UserRefreshToken userRefreshToken = await _refreshTokenRepository.AddRefreshToken(jwtToken, refreshToken, email, ipAddress);
			return userRefreshToken.RefreshToken;
		}

		public string CreateJwtToken(IList<Claim> existingClaims, string email)
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
				Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.JwtExpiresMinutes),
				SigningCredentials =
					new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
			};

			SecurityToken token = _tokenHandler.CreateToken(tokenDescriptor);
			return _tokenHandler.WriteToken(token);
		}
	}
}
