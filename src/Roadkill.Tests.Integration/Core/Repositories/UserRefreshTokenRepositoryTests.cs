using System;
using System.Threading.Tasks;
using AutoFixture;
using Marten;
using Roadkill.Core.Authorization;
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
		public async Task Should_add_and_get_token()
		{
			// given
			var now = DateTime.UtcNow;
			string email = "stuart@minions.com";
			UserRefreshTokenRepository repository = CreateRepository();

			// when
			await repository.AddRefreshToken(email);
			UserRefreshToken token = await repository.GetRefreshToken(email);

			// then
			token.ShouldNotBeNull();
			token.Email.ShouldBe(email);
			token.RefreshToken.ShouldNotBeNullOrEmpty();
			token.CreationDate.ShouldBeGreaterThanOrEqualTo(now);
		}
	}
}
