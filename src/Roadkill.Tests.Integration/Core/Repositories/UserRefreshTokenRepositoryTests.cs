using System;
using System.Threading.Tasks;
using AutoFixture;
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
		private readonly Fixture _fixture;
		private readonly ITestOutputHelper _outputHelper;

		public UserRefreshTokenRepositoryTests(ITestOutputHelper outputHelper)
		{
			_fixture = new Fixture();
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
		public async Task Should_add_and_get_token_per_device()
		{
			// given
			var now = DateTime.UtcNow;
			string email = "stuart@minions.com";

			string firstRefreshToken = "F5 F5";
			string firstDeviceIpAddress = "8.8.8.8";

			string secondRefreshToken = "F6 F6";
			string secondDeviceIpAddress = "8.8.8.9";

			UserRefreshTokenRepository repository = CreateRepository();

			// when
			UserRefreshToken token1 = await repository.AddRefreshToken(email, firstRefreshToken, firstDeviceIpAddress);
			UserRefreshToken token2 = await repository.AddRefreshToken(email, secondRefreshToken, secondDeviceIpAddress);

			UserRefreshToken firstToken = await repository.GetByRefreshToken(token1.RefreshToken, firstDeviceIpAddress);

			// then
			firstToken.ShouldNotBeNull();
			firstToken.Email.ShouldBe(email);
			firstToken.RefreshToken.ShouldBe(firstRefreshToken);
			firstToken.CreationDate.ShouldBeGreaterThanOrEqualTo(now);
			firstToken.IpAddress.ShouldBe(firstDeviceIpAddress);
		}

		[Fact]
		public async Task Should_remove_token_per_device()
		{
			// given
			var now = DateTime.UtcNow;
			string email = "stuart@minions.com";

			string firstRefreshToken = "F5 F5";
			string firstDeviceIpAddress = "8.8.8.8";

			string secondRefreshToken = "F6 F6";
			string secondDeviceIpAddress = "8.8.8.9";

			UserRefreshTokenRepository repository = CreateRepository();

			// when
			var userToken1 = await repository.AddRefreshToken(email, firstRefreshToken, firstDeviceIpAddress);
			var userToken2 = await repository.AddRefreshToken(email, secondRefreshToken, secondDeviceIpAddress);

			await repository.DeleteRefreshTokens(email, firstDeviceIpAddress);

			// then
			UserRefreshToken token1 = await repository.GetByRefreshToken(userToken1.RefreshToken, firstDeviceIpAddress);
			UserRefreshToken token2 = await repository.GetByRefreshToken(userToken2.RefreshToken, secondDeviceIpAddress);

			token1.ShouldBeNull();
			token2.ShouldNotBeNull();
		}
	}
}
