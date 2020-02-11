using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Baseline;
using Marten;
using Roadkill.Core.Entities.Authorization;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Core.Repositories
{
	public class UserRefreshTokenRepositoryTests
	{
		private readonly ITestOutputHelper _outputHelper;

		public UserRefreshTokenRepositoryTests(ITestOutputHelper outputHelper)
		{
			_outputHelper = outputHelper;
			IDocumentStore documentStore = DocumentStoreManager.GetMartenDocumentStore(typeof(UserRefreshTokenRepositoryTests), outputHelper);

			try
			{
				new UserRepository(documentStore).Wipe();
			}
			catch (Exception e)
			{
				outputHelper.WriteLine(GetType().Name + " caught: " + e.Message);
			}
		}

		public UserRefreshTokenRepository CreateRepository()
		{
			IDocumentStore documentStore = DocumentStoreManager.GetMartenDocumentStore(typeof(UserRefreshTokenRepositoryTests), _outputHelper);
			return new UserRefreshTokenRepository(documentStore);
		}

		[Fact]
		public async Task Should_add_token_and_fill_properties()
		{
			// given
			var now = DateTime.UtcNow;
			string jwtToken = "jwt";
			string email = "stuart@minions.com";
			string refreshToken = "token stuart 1";
			string ip = "ip1";

			UserRefreshTokenRepository repository = CreateRepository();

			// when
			await repository.AddRefreshToken(jwtToken, refreshToken, email, ip);

			// then
			UserRefreshToken token = await repository.GetRefreshToken(refreshToken);
			token.ShouldNotBeNull();
			token.JwtToken.ShouldBe(jwtToken);
			token.RefreshToken.ShouldBe(refreshToken);
			token.CreationDate.ShouldBeGreaterThanOrEqualTo(now);
			token.IpAddress.ShouldBe(ip);
			token.Email.ShouldBe(email);
		}

		[Fact]
		public async Task Should_remove_token()
		{
			// given
			string email1 = "stuart@minions.com";
			string refreshToken = "token stuart 1";

			UserRefreshTokenRepository repository = CreateRepository();
			await repository.AddRefreshToken("jwt1", refreshToken, email1, "ip1");

			// when
			await repository.DeleteRefreshToken(refreshToken);

			// then
			UserRefreshToken deletedToken = await repository.GetRefreshToken(refreshToken);
			deletedToken.ShouldBeNull();
		}

		[Fact]
		public async Task Should_remove_tokens_for_user_email()
		{
			// given
			string email1 = "stuart@minions.com";
			string tokenForUser1 = "token stuart 1";
			string tokenForUser1_b = "token stuart 2";

			string email2 = "bob@minions.com";
			string tokenForUser2 = "token for bob";

			UserRefreshTokenRepository repository = CreateRepository();
			await repository.AddRefreshToken("jwt1", tokenForUser1, email1, "ip1");
			await repository.AddRefreshToken("jwt2", tokenForUser1_b, email1, "ip2");
			await repository.AddRefreshToken("jwt3", tokenForUser2, email2, "ip3");

			// when
			await repository.DeleteRefreshTokens(email1);

			// then
			UserRefreshToken deletedToken1 = await repository.GetRefreshToken(tokenForUser1);
			UserRefreshToken deletedToken2 = await repository.GetRefreshToken(tokenForUser1_b);
			UserRefreshToken activeToken = await repository.GetRefreshToken(tokenForUser2);

			deletedToken1.ShouldBeNull();
			deletedToken2.ShouldBeNull();

			activeToken.ShouldNotBeNull();
		}
	}
}
