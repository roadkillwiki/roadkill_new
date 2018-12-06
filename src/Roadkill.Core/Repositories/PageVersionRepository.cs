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
		Task<PageVersion> AddNewVersion(int pageId, string text, string author, DateTime? dateTime = null);

		Task<IEnumerable<PageVersion>> AllVersions();

		Task DeleteVersion(Guid id);

		Task<IEnumerable<PageVersion>> FindPageVersionsByPageId(int pageId);

		Task<IEnumerable<PageVersion>> FindPageVersionsByAuthor(string username);

		Task<PageVersion> GetLatestVersion(int pageId);

		Task<PageVersion> GetById(Guid id);

		// doesn't add a new version
		Task UpdateExistingVersion(PageVersion version);
	}

	public class PageVersionRepository : IPageVersionRepository
	{
		private readonly IDocumentStore _store;

		public PageVersionRepository(IDocumentStore store)
		{
			_store = store ?? throw new ArgumentNullException(nameof(store));
		}

		public async Task<PageVersion> AddNewVersion(int pageId, string text, string author, DateTime? dateTime = null)
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

		public async Task<IEnumerable<PageVersion>> AllVersions()
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<PageVersion>()
					.ToListAsync();
			}
		}

		public async Task DeleteVersion(Guid id)
		{
			using (var session = _store.OpenSession())
			{
				session.Delete<PageVersion>(id);
				session.DeleteWhere<PageVersion>(x => x.Id == id);
				await session.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<PageVersion>> FindPageVersionsByPageId(int pageId)
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

		public async Task<IEnumerable<PageVersion>> FindPageVersionsByAuthor(string username)
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

		public async Task<PageVersion> GetLatestVersion(int pageId)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<PageVersion>()
					.OrderByDescending(x => x.DateTime)
					.FirstOrDefaultAsync(x => x.PageId == pageId);
			}
		}

		public async Task<PageVersion> GetById(Guid id)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<PageVersion>()
					.FirstOrDefaultAsync(x => x.Id == id);
			}
		}

		public async Task UpdateExistingVersion(PageVersion version)
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
