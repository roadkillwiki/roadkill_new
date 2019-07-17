using System;
using System.Threading.Tasks;
using Marten;
using Roadkill.Core.Authorization;

namespace Roadkill.Core.Repositories
{
	public interface IUserRefreshTokenRepository
	{
		Task<UserRefreshToken> GetRefreshToken(string email);
		Task AddRefreshToken(string email);
	}

	public class UserRefreshTokenRepository : IUserRefreshTokenRepository
	{
		private readonly IDocumentStore _store;

		public UserRefreshTokenRepository(IDocumentStore store)
		{
			_store = store ?? throw new ArgumentNullException(nameof(store));
		}

		public async Task<UserRefreshToken> GetRefreshToken(string email)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<UserRefreshToken>()
					.FirstOrDefaultAsync(x => x.Email == email);
			}
		}

		public async Task AddRefreshToken(string email)
		{
			using (var session = _store.LightweightSession())
			{
				var token = new UserRefreshToken()
				{
					Email = email,
					CreationDate = DateTime.UtcNow,
					RefreshToken = Guid.NewGuid().ToString("N")
				};
				session.Store(token);
				await session.SaveChangesAsync();
			}
		}
	}
}
