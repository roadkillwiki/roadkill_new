using System;
using System.Threading.Tasks;
using Marten;
using Roadkill.Core.Entities.Authorization;

namespace Roadkill.Core.Repositories
{
	public interface IUserRefreshTokenRepository
	{
		Task<UserRefreshToken> GetRefreshToken(string email, string ipAddress);
		Task<UserRefreshToken> AddRefreshToken(string email, string refreshToken, string ipAddress);
		Task DeleteRefreshTokens(string email, string ipAddress);
	}

	public class UserRefreshTokenRepository : IUserRefreshTokenRepository
	{
		private readonly IDocumentStore _store;

		public UserRefreshTokenRepository(IDocumentStore store)
		{
			_store = store ?? throw new ArgumentNullException(nameof(store));
		}

		public async Task<UserRefreshToken> GetRefreshToken(string email, string ipAddress)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<UserRefreshToken>()
					.FirstOrDefaultAsync(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
					                          x.IpAddress == ipAddress);
			}
		}

		public async Task<UserRefreshToken> AddRefreshToken(string email, string refreshToken, string ipAddress)
		{
			using (var session = _store.LightweightSession())
			{
				var token = new UserRefreshToken()
				{
					Id = Guid.NewGuid().ToString(),
					Email = email,
					CreationDate = DateTime.UtcNow,
					RefreshToken = refreshToken,
					IpAddress = ipAddress
				};
				session.Store(token);
				await session.SaveChangesAsync();

				return token;
			}
		}

		public async Task DeleteRefreshTokens(string email, string ipAddress)
		{
			using (var session = _store.LightweightSession())
			{
				session.DeleteWhere<UserRefreshToken>(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
				                                           x.IpAddress == ipAddress);
				await session.SaveChangesAsync();
			}
		}
	}
}
