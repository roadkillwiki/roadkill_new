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
		Task DeleteAllUsers();

		Task DeleteUser(User user);

		Task<IEnumerable<User>> FindAllEditors();

		Task<IEnumerable<User>> FindAllAdmins();

		Task<User> GetAdminById(Guid id);

		Task<User> GetUserByActivationKey(string key);

		Task<User> GetEditorById(Guid id);

		Task<User> GetUserByEmail(string email, bool? isActivated = null);

		Task<User> GetUserById(Guid id, bool? isActivated = null);

		Task<User> GetUserByPasswordResetKey(string key);

		Task<User> GetUserByUsername(string username);

		Task<User> GetUserByUsernameOrEmail(string username, string email);

		Task SaveOrUpdateUser(User user);
	}

	public class UserRepository : IUserRepository
	{
		private readonly IDocumentStore _store;

		public UserRepository(IDocumentStore store)
		{
			if (store == null)
				throw new ArgumentNullException(nameof(store));

			_store = store;
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

		public async Task DeleteAllUsers()
		{
			using (IDocumentSession session = _store.LightweightSession())
			{
				session.DeleteWhere<User>(x => true);
				await session.SaveChangesAsync();
			}
		}

		public async Task DeleteUser(User user)
		{
			using (IDocumentSession session = _store.LightweightSession())
			{
				session.DeleteWhere<User>(x => x.Id == user.Id);
				await session.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<User>> FindAllEditors()
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.Where(x => x.IsEditor)
					.ToListAsync();
			}
		}

		public async Task<IEnumerable<User>> FindAllAdmins()
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.Where(x => x.IsAdmin)
					.ToListAsync();
			}
		}

		public async Task<User> GetAdminById(Guid id)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.Id == id);
			}
		}

		public async Task<User> GetUserByActivationKey(string key)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.ActivationKey == key);
			}
		}

		public async Task<User> GetEditorById(Guid id)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
						.Query<User>()
						.FirstOrDefaultAsync(x => x.Id == id && x.IsEditor);
			}
		}

		public async Task<User> GetUserByEmail(string email, bool? isActivated = null)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.Email == email && x.IsActivated == isActivated);
			}
		}

		public async Task<User> GetUserById(Guid id, bool? isActivated = null)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.Id == id && x.IsActivated == (isActivated ?? true));
			}
		}

		public async Task<User> GetUserByPasswordResetKey(string key)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.PasswordResetKey == key);
			}
		}

		public async Task<User> GetUserByUsername(string username)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.Username == username);
			}
		}

		public async Task<User> GetUserByUsernameOrEmail(string username, string email)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<User>()
					.FirstOrDefaultAsync(x => x.Username == username || x.Email == email);
			}
		}

		public async Task SaveOrUpdateUser(User user)
		{
			using (IDocumentSession session = _store.LightweightSession())
			{
				session.Store(user);
				await session.SaveChangesAsync();
			}
		}
	}
}