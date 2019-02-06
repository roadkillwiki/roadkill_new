using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Roadkill.Api.JWT;
using Roadkill.Api.Settings;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.JWT
{
	public class JwtTokenProviderTests
	{
		private JwtSettings _jwtSettings;
		private SecurityTokenHandler _tokenHandler;
		private JwtTokenProvider _provider;

		public JwtTokenProviderTests()
		{
			// This token is from jwt.io, the format:
			// token = base64urlEncoding(header) + '.' + base64urlEncoding(payload) + '.' + base64urlEncoding(signature)
			string exampleJwtToken =
				"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
			var securityToken = new JwtSecurityToken(exampleJwtToken);

			_tokenHandler = Substitute.For<SecurityTokenHandler>();
			_tokenHandler
				.CreateToken(Arg.Any<SecurityTokenDescriptor>())
				.Returns(securityToken);
			_tokenHandler
				.WriteToken(securityToken)
				.Returns("the jwt token");

			_jwtSettings = new JwtSettings()
			{
				Password = "a-very-secure-password-18-characters",
				ExpireDays = 10
			};

			_provider = new JwtTokenProvider(_jwtSettings, _tokenHandler);
		}

		[Fact]
		public void should_add_email_to_existing_claims_stored_in_token()
		{
			// given
			var roleClaim = new Claim(ClaimTypes.Role, "Admin");
			var existingClaims = new List<Claim>() { roleClaim };
			string email = "bob@example.com";

			// when
			string token = _provider.CreateToken(existingClaims, email);

			// then
			token.ShouldNotBeNullOrEmpty();
			_tokenHandler
				.Received()
				.CreateToken(Arg.Is<SecurityTokenDescriptor>(
					x => x.Subject.Claims.Any(y => y.Type == roleClaim.Type && y.Value == roleClaim.Value) &&
					     x.Subject.Claims.Any(y => y.Type == ClaimTypes.Name && y.Value == email)));
		}

		[Fact]
		public void should_set_expiry_time_stored_in_token()
		{
			// given
			var existingClaims = new List<Claim>() { new Claim(ClaimTypes.Role, "Admin") };
			string email = "bob@example.com";
			var expectedExpiry = DateTime.UtcNow.AddDays(_jwtSettings.ExpireDays);

			// when
			string token = _provider.CreateToken(existingClaims, email);

			// then
			token.ShouldNotBeNullOrEmpty();
			_tokenHandler
				.Received()
				.CreateToken(Arg.Is<SecurityTokenDescriptor>(x => x.Expires >= expectedExpiry));
		}
	}
}
