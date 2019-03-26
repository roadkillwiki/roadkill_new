using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Roadkill.Core.Entities;

namespace Roadkill.Core.Search.Adapters
{
	public interface ISearchAdapter
	{
		Task<bool> Add(SearchablePage page);

		Task<bool> Update(SearchablePage page);

		Task RecreateIndex();

		Task<IEnumerable<SearchablePage>> Find(string query);
	}

	public class PostgresSearchAdapter : ISearchAdapter
	{
		private readonly IDocumentStore _documentStore;

		public PostgresSearchAdapter(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public async Task<bool> Add(SearchablePage page)
		{
			using (var session = _documentStore.LightweightSession())
			{
				session.Store(page);

				await session.SaveChangesAsync();
				return true;
			}
		}

		public async Task<bool> Update(SearchablePage page)
		{
			using (var session = _documentStore.LightweightSession())
			{
				session.Delete<SearchablePage>(page.Id);
				session.Store(page);

				await session.SaveChangesAsync();
				return true;
			}
		}

		public async Task RecreateIndex()
		{
			using (var session = _documentStore.LightweightSession())
			{
				session.DeleteWhere<SearchablePage>(x => true);

				// TODO: add all pages back in
				await session.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<SearchablePage>> Find(string query)
		{
			using (var session = _documentStore.QuerySession())
			{
				return await session.Query<SearchablePage>()
					.Where(x => x.Text.Contains(query))
					.ToListAsync();
			}
		}
	}
}
