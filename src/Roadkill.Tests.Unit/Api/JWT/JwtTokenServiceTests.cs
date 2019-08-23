using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Roadkill.Api.JWT;
using Roadkill.Api.Settings;
using Roadkill.Core.Entities.Authorization;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.JWT
{
	public class JwtTokenServiceTests
	{
		private JwtSettings _jwtSettings;
		private SecurityTokenHandler _tokenHandler;
		private JwtTokenService _service;
		private IUserRefreshTokenRepository _refreshTokenRepository;

		public JwtTokenServiceTests()
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
				ExpiresDays = 10
			};

			_refreshTokenRepository = Substitute.For<IUserRefreshTokenRepository>();
			_service = new JwtTokenService(_jwtSettings, _tokenHandler, _refreshTokenRepository);
		}

		[Fact]
		public void should_add_email_to_existing_claims_stored_in_token()
		{
			// given
			var roleClaim = new Claim(ClaimTypes.Role, "Admin");
			var existingClaims = new List<Claim>() { roleClaim };
			string email = "bob@example.com";

			// when
			string token = _service.CreateToken(existingClaims, email);

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
			var expectedExpiry = DateTime.UtcNow.AddDays(_jwtSettings.ExpiresDays);

			// when
			string token = _service.CreateToken(existingClaims, email);

			// then
			token.ShouldNotBeNullOrEmpty();
			_tokenHandler
				.Received()
				.CreateToken(Arg.Is<SecurityTokenDescriptor>(x => x.Expires >= expectedExpiry));
		}

		[Fact]
		public async Task should_save_refresh_token_and_return_repositorys_token()
		{
			// given
			string expectedToken = "refresh";
			string email = "bob@example.com";
			string ipAddress = "127.1.2.3";

			_refreshTokenRepository.AddRefreshToken(email, Arg.Any<string>(), ipAddress)
				.Returns(new UserRefreshToken()
				{
					RefreshToken = expectedToken
				});

			// when
			string token = await _service.CreateRefreshToken(email, ipAddress);

			// then
			token.ShouldBe(expectedToken);
		}

		[Fact]
		public async Task GetEmailByRefreshToken_should_find_email_by_token_and_ip()
		{
			// given
			string refreshToken = "old refresh token";
			string ipAddress = "127.1.2.3";
			string expectedEmail = "bob@example.com";

			_refreshTokenRepository.GetByRefreshToken(refreshToken, ipAddress)
				.Returns(new UserRefreshToken()
				{
					Email = expectedEmail
				});

			// when
			string email = await _service.GetEmailByRefreshToken(refreshToken, ipAddress);

			// then
			email.ShouldBe(expectedEmail);
		}
	}
}
