using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Roadkill.Api.Settings;

namespace Roadkill.Api.JWT
{
	public class JwtTokenProvider : IJwtTokenProvider
	{
		private readonly JwtSettings _jwtSettings;
		private readonly SecurityTokenHandler _tokenHandler;

		public JwtTokenProvider(
			JwtSettings jwtSettings,
			SecurityTokenHandler tokenHandler)
		{
			_jwtSettings = jwtSettings;
			_tokenHandler = tokenHandler;
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
				Expires = DateTime.UtcNow.AddDays(_jwtSettings.ExpireDays),
				SigningCredentials =
					new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
			};

			SecurityToken token = _tokenHandler.CreateToken(tokenDescriptor);
			return _tokenHandler.WriteToken(token);
		}
	}
}
