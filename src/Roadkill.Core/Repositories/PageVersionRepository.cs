using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Roadkill.Core.Entities;

namespace Roadkill.Core.Repositories
{
	public interface IPageVersionRepository
	{
		Task<PageVersion> AddNewVersionAsync(int pageId, string text, string author, DateTime? dateTime = null);

		Task<IEnumerable<PageVersion>> AllVersionsAsync();

		Task DeleteVersionAsync(Guid id);

		Task<IEnumerable<PageVersion>> FindPageVersionsByPageIdAsync(int pageId);

		Task<IEnumerable<PageVersion>> FindPageVersionsByAuthorAsync(string username);

		Task<PageVersion> GetLatestVersionAsync(int pageId);

		Task<PageVersion> GetByIdAsync(Guid id);

		// doesn't add a new version
		Task UpdateExistingVersionAsync(PageVersion version);
	}

	public class PageVersionRepository : IPageVersionRepository
	{
		private readonly IDocumentStore _store;

		public PageVersionRepository(IDocumentStore store)
		{
			_store = store ?? throw new ArgumentNullException(nameof(store));
		}

		public async Task<PageVersion> AddNewVersionAsync(int pageId, string text, string author, DateTime? dateTime = null)
		{
			using (var session = _store.LightweightSession())
			{
				if (dateTime == null)
				{
					dateTime = DateTime.UtcNow;
				}

				var pageVersion = new PageVersion
				{
					Id = Guid.NewGuid(),
					Author = author,
					DateTime = dateTime.Value,
					PageId = pageId,
					Text = text
				};
				session.Store(pageVersion);

				await session.SaveChangesAsync();
				return pageVersion;
			}
		}

		public async Task<IEnumerable<PageVersion>> AllVersionsAsync()
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<PageVersion>()
					.ToListAsync();
			}
		}

		public async Task DeleteVersionAsync(Guid id)
		{
			using (var session = _store.OpenSession())
			{
				session.Delete<PageVersion>(id);
				session.DeleteWhere<PageVersion>(x => x.Id == id);
				await session.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<PageVersion>> FindPageVersionsByPageIdAsync(int pageId)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<PageVersion>()
					.Where(x => x.PageId == pageId)
					.OrderByDescending(x => x.DateTime)
					.ToListAsync();
			}
		}

		public async Task<IEnumerable<PageVersion>> FindPageVersionsByAuthorAsync(string username)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<PageVersion>()
					.Where(x => x.Author.Equals(username, StringComparison.CurrentCultureIgnoreCase))
					.OrderByDescending(x => x.DateTime)
					.ToListAsync();
			}
		}

		public async Task<PageVersion> GetLatestVersionAsync(int pageId)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<PageVersion>()
					.OrderByDescending(x => x.DateTime)
					.FirstOrDefaultAsync(x => x.PageId == pageId);
			}
		}

		public async Task<PageVersion> GetByIdAsync(Guid id)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<PageVersion>()
					.FirstOrDefaultAsync(x => x.Id == id);
			}
		}

		public async Task UpdateExistingVersionAsync(PageVersion version)
		{
			using (var session = _store.LightweightSession())
			{
				session.Store(version);
				await session.SaveChangesAsync();
			}
		}

		public void Wipe()
		{
			try
			{
				_store.Advanced.Clean.DeleteDocumentsFor(typeof(PageVersion));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
