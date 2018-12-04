using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Roadkill.Core.Entities;

namespace Roadkill.Core.Repositories
{
	public interface IPageRepository
	{
		Task<Page> AddNewPage(Page page);

		Task<IEnumerable<Page>> AllPages();

		Task<IEnumerable<string>> AllTags();

		Task DeletePage(int id);

		Task DeleteAllPages();

		Task<IEnumerable<Page>> FindPagesCreatedBy(string username);

		Task<IEnumerable<Page>> FindPagesLastModifiedBy(string username);

		Task<IEnumerable<Page>> FindPagesContainingTag(string tag);

		Task<Page> GetPageById(int id);

		// Case insensitive search by page title
		Task<Page> GetPageByTitle(string title);

		Task<Page> UpdateExisting(Page page);
	}

	public class PageRepository : IPageRepository
	{
		private readonly IDocumentStore _store;

		public PageRepository(IDocumentStore store)
		{
			if (store == null)
				throw new ArgumentNullException(nameof(store));

			_store = store;
		}

		public void Wipe()
		{
			try
			{
				_store.Advanced.Clean.DeleteDocumentsFor(typeof(Page));
				_store.Advanced.Clean.DeleteDocumentsFor(typeof(PageVersion));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public async Task<Page> AddNewPage(Page page)
		{
			page.Id = 0; // reset so it's autoincremented

			using (IDocumentSession session = _store.LightweightSession())
			{
				session.Store(page);

				await session.SaveChangesAsync();
				return page;
			}
		}

		public async Task<IEnumerable<Page>> AllPages()
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.ToListAsync();
			}
		}

		public async Task<IEnumerable<string>> AllTags()
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.Select(x => x.Tags)
					.ToListAsync();
			}
		}

		public async Task DeletePage(int pageId)
		{
			using (IDocumentSession session = _store.LightweightSession())
			{
				session.Delete<Page>(pageId);
				await session.SaveChangesAsync();
			}
		}

		public async Task DeleteAllPages()
		{
			using (IDocumentSession session = _store.LightweightSession())
			{
				session.DeleteWhere<Page>(x => true);
				session.DeleteWhere<PageVersion>(x => true);

				await session.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<Page>> FindPagesCreatedBy(string username)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.Where(x => x.CreatedBy.Equals(username, StringComparison.CurrentCultureIgnoreCase))
					.ToListAsync();
			}
		}

		public async Task<IEnumerable<Page>> FindPagesLastModifiedBy(string username)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.Where(x => x.LastModifiedBy.Equals(username, StringComparison.CurrentCultureIgnoreCase))
					.ToListAsync();
			}
		}

		public async Task<IEnumerable<Page>> FindPagesContainingTag(string tag)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.Where(x => x.Tags.Contains(tag))
					.ToListAsync();
			}
		}

		public async Task<Page> GetPageById(int id)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.FirstOrDefaultAsync(x => x.Id == id);
			}
		}

		public async Task<Page> GetPageByTitle(string title)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.FirstOrDefaultAsync(x => x.Title == title);
			}
		}

		public async Task<Page> UpdateExisting(Page page)
		{
			using (IDocumentSession session = _store.LightweightSession())
			{
				session.Update(page);

				await session.SaveChangesAsync();
				return page;
			}
		}
	}
}