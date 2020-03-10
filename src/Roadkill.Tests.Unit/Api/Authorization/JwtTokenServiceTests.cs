using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Roadkill.Api.Authorization.JWT;
using Roadkill.Api.Authorization.Roles;
using Roadkill.Api.Settings;
using Roadkill.Core.Entities.Authorization;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Authorization
{
	public class JwtTokenServiceTests
	{
		private readonly JwtSettings _jwtSettings;
		private readonly SecurityTokenHandler _tokenHandler;
		private readonly JwtTokenService _service;
		private readonly IUserRefreshTokenRepository _refreshTokenRepository;
		private readonly TokenValidationParameters _jwtTokenValidationParameters;

		private readonly string _fakeJwtToken = "the jwt token";
		private readonly int _refreshTokenExpiresDays = 7;

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
				.Returns(_fakeJwtToken);

			_jwtSettings = new JwtSettings()
			{
				Password = "a-very-secure-password-18-characters",
				JwtExpiresMinutes = 5,
				RefreshTokenExpiresDays = _refreshTokenExpiresDays
			};

			_jwtTokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Password)),
				ValidateIssuer = false,
				ValidateAudience = false,
				RequireExpirationTime = false,
				ValidateLifetime = true
			};

			_refreshTokenRepository = Substitute.For<IUserRefreshTokenRepository>();
			_service = new JwtTokenService(_jwtSettings, _tokenHandler, _refreshTokenRepository, _jwtTokenValidationParameters);
		}

		[Fact]
		public void should_add_email_to_existing_claims_stored_in_token()
		{
			// given
			var roleClaim = RoadkillClaims.AdminClaim;
			var existingClaims = new List<Claim>() { roleClaim };
			string email = "bob@example.com";

			// when
			string token = _service.CreateJwtToken(existingClaims, email);

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
			var existingClaims = new List<Claim>() { RoadkillClaims.AdminClaim };
			string email = "bob@example.com";
			var expectedExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.JwtExpiresMinutes);

			// when
			string token = _service.CreateJwtToken(existingClaims, email);

			// then
			token.ShouldNotBeNullOrEmpty();
			_tokenHandler
				.Received()
				.CreateToken(Arg.Is<SecurityTokenDescriptor>(x => x.Expires >= expectedExpiry));
		}

		[Fact]
		public async Task should_save_refresh_token_and_return_repository_token()
		{
			// given
			string jwtToken = "jwt";
			string refreshToken = "refresh";
			string email = "bob@example.com";
			string ipAddress = "127.1.2.3";

			_refreshTokenRepository.AddRefreshToken(jwtToken, refreshToken, email, ipAddress)
				.Returns(new UserRefreshToken()
				{
					RefreshToken = jwtToken
				});

			// when
			string token = await _service.StoreRefreshToken(jwtToken, refreshToken, email, ipAddress);

			// then
			token.ShouldBe(jwtToken);
		}

		[Fact]
		public async Task should_get_jwt_token()
		{
			// given
			string currentJwtToken = "jwt token";
			var adminClaim = new Claim(ClaimTypes.Role, AdminRoleDefinition.Name);
			var claimsList = new List<Claim>() { adminClaim };
			var fakeJwtSecurityToken = new JwtSecurityToken(claims: claimsList);

			_tokenHandler
				.ValidateToken(currentJwtToken, _jwtTokenValidationParameters, out Arg.Any<SecurityToken>())
				.Returns(x =>
				{
					x[2] = fakeJwtSecurityToken; // set the out (3rd parameter) of ValidateToken()
					return new ClaimsPrincipal();
				});

			// when
			JwtSecurityToken jwtSecurityToken = _service.GetJwtSecurityToken(currentJwtToken);

			// then
			jwtSecurityToken.ShouldNotBeNull();

			var actualClaim = jwtSecurityToken.Claims.FirstOrDefault();
			actualClaim.ShouldNotBeNull();
			actualClaim.Type.ShouldBe(adminClaim.Type);
			actualClaim.Value.ShouldBe(adminClaim.Value);
		}

		[Fact]
		public async Task GetRefreshToken_should_get_from_existing_jwt_token()
		{
			// given
			string existingRefreshToken = "a fake GUID";
			var expectedToken = new UserRefreshToken()
			{
				Email = "test@localhost",
				CreationDate = DateTime.UtcNow.AddDays(-1),
				IpAddress = "1.0.0.0",
				JwtToken = "jwt token",
				RefreshToken = existingRefreshToken
			};

			_refreshTokenRepository
				.GetRefreshToken(existingRefreshToken)
				.Returns(expectedToken);

			_tokenHandler
				.ValidateToken(expectedToken.JwtToken, _jwtTokenValidationParameters, out Arg.Any<SecurityToken>())
				.Returns(x =>
				{
					var adminClaim = new Claim(ClaimTypes.Role, AdminRoleDefinition.Name);
					var claimsList = new List<Claim>() { adminClaim };
					var fakeJwtSecurityToken = new JwtSecurityToken(claims: claimsList);

					x[2] = fakeJwtSecurityToken; // set the out (3rd parameter) of ValidateToken()
					return new ClaimsPrincipal();
				});

			// when
			var userRefreshToken = await _service.GetExistingRefreshToken(existingRefreshToken);

			// then
			userRefreshToken.ShouldNotBeNull();
			userRefreshToken.Email.ShouldBe(expectedToken.Email);
		}

		[Fact]
		public async Task GetRefreshToken_should_ignore_expired_tokens()
		{
			// given
			string existingRefreshToken = "a fake GUID";
			var expectedToken = new UserRefreshToken()
			{
				Email = "test@localhost",
				CreationDate = DateTime.UtcNow.AddDays(_refreshTokenExpiresDays +1),
				IpAddress = "1.0.0.0",
				JwtToken = "jwt token",
				RefreshToken = existingRefreshToken
			};

			_refreshTokenRepository
				.GetRefreshToken(existingRefreshToken)
				.Returns(expectedToken);

			// when
			var userRefreshToken = await _service.GetExistingRefreshToken(existingRefreshToken);

			// then
			userRefreshToken.ShouldBeNull();
		}
	}
}
