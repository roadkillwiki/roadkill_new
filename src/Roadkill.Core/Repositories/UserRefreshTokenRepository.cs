using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Marten;
using Roadkill.Core.Entities.Authorization;

namespace Roadkill.Core.Repositories
{
	public interface IUserRefreshTokenRepository
	{
		Task<UserRefreshToken> GetRefreshToken(string refreshToken);
		Task<UserRefreshToken> AddRefreshToken(string jwtId, string refreshToken, string email, string ipAddress);
		Task DeleteRefreshToken(string refreshToken);
		Task DeleteRefreshTokens(string email);
	}

	public class UserRefreshTokenRepository : IUserRefreshTokenRepository
	{
		private readonly IDocumentStore _store;

		public UserRefreshTokenRepository(IDocumentStore store)
		{
			_store = store ?? throw new ArgumentNullException(nameof(store));
		}

		public async Task<UserRefreshToken> GetRefreshToken(string refreshToken)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<UserRefreshToken>()
					.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
			}
		}

		public async Task<UserRefreshToken> AddRefreshToken(string jwtId, string refreshToken, string email, string ipAddress)
		{
			using (var session = _store.LightweightSession())
			{
				var token = new UserRefreshToken()
				{
					JwtToken = jwtId,
					RefreshToken = refreshToken,
					CreationDate = DateTime.UtcNow,
					IpAddress = ipAddress,
					Email = email
				};
				session.Store(token);
				await session.SaveChangesAsync();

				return token;
			}
		}

		public async Task DeleteRefreshToken(string refreshToken)
		{
			using (var session = _store.LightweightSession())
			{
				session.DeleteWhere<UserRefreshToken>(x => x.RefreshToken == refreshToken);
				await session.SaveChangesAsync();
			}
		}

		public async Task DeleteRefreshTokens(string email)
		{
			using (var session = _store.LightweightSession())
			{
				session.DeleteWhere<UserRefreshToken>(x => x.Email== email);
				await session.SaveChangesAsync();
			}
		}
	}
}
