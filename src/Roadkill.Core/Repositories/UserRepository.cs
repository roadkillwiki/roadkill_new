using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Roadkill.Core.Entities;

namespace Roadkill.Core.Repositories
{
	public interface IUserRepository
	{
		Task DeleteAllUsersAsync();

		Task DeleteUserAsync(User user);

		Task<IEnumerable<User>> FindAllEditorsAsync();

		Task<IEnumerable<User>> FindAllAdminsAsync();

		Task<User> GetAdminByIdAsync(Guid id);

		Task<User> GetUserByActivationKeyAsync(string key);

		Task<User> GetEditorByIdAsync(Guid id);

		Task<User> GetUserByEmailAsync(string email, bool? isActivated = null);

		Task<User> GetUserByIdAsync(Guid id, bool? isActivated = null);

		Task<User> GetUserByPasswordResetKeyAsync(string key);

		Task<User> GetUserByUsernameAsync(string username);

		Task<User> GetUserByUsernameOrEmailAsync(string username, string email);

		Task SaveOrUpdateUserAsync(User user);
	}

	public class UserRepository : IUserRepository
	{
		private readonly IDocumentStore _store;

		public UserRepository(IDocumentStore store)
		{
			_store = store ?? throw new ArgumentNullException(nameof(store));
		}

		public async Task DeleteAllUsersAsync()
		{
			using (var session = _store.LightweightSession())
			{
				session.DeleteWhere<User>(x => true);
				await session.SaveChangesAsync();
			}
		}

		public async Task DeleteUserAsync(User user)
		{
			using (var session = _store.LightweightSession())
			{
				session.DeleteWhere<User>(x => x.Id == user.Id);
				await session.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<User>> FindAllEditorsAsync()
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.Where(x => x.IsEditor)
					.ToListAsync();
			}
		}

		public async Task<IEnumerable<User>> FindAllAdminsAsync()
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.Where(x => x.IsAdmin)
					.ToListAsync();
			}
		}

		public async Task<User> GetAdminByIdAsync(Guid id)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.Id == id);
			}
		}

		public async Task<User> GetUserByActivationKeyAsync(string key)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.ActivationKey == key);
			}
		}

		public async Task<User> GetEditorByIdAsync(Guid id)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.Id == id && x.IsEditor);
			}
		}

		public async Task<User> GetUserByEmailAsync(string email, bool? isActivated = null)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.Email == email && x.IsActivated == isActivated);
			}
		}

		public async Task<User> GetUserByIdAsync(Guid id, bool? isActivated = null)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.Id == id && x.IsActivated == (isActivated ?? true));
			}
		}

		public async Task<User> GetUserByPasswordResetKeyAsync(string key)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.PasswordResetKey == key);
			}
		}

		public async Task<User> GetUserByUsernameAsync(string username)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.Username == username);
			}
		}

		public async Task<User> GetUserByUsernameOrEmailAsync(string username, string email)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.Username == username || x.Email == email);
			}
		}

		public async Task SaveOrUpdateUserAsync(User user)
		{
			using (var session = _store.LightweightSession())
			{
				session.Store(user);
				await session.SaveChangesAsync();
			}
		}

		public void Wipe()
		{
			try
			{
				_store.Advanced.Clean.DeleteDocumentsFor(typeof(User));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
