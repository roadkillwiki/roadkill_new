using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Roadkill.Core.Entities;
using Roadkill.Core.Search.Parsers;

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
			var p = new SearchQueryParser();
			ParsedQueryResult queryResult = p.ParseQuery(query);

			using (var session = _documentStore.QuerySession())
			{
				var martenQuery = session.Query<SearchablePage>().Where(x => true);

				if (!string.IsNullOrEmpty(queryResult.TextWithoutFields))
				{
					martenQuery = martenQuery.Where(x => x.PlainTextSearch(queryResult.TextWithoutFields));
				}

				string title = queryResult.GetFieldValue("title");
				if (!string.IsNullOrEmpty(title))
				{
					martenQuery = martenQuery.Where(x => x.Title.PlainTextSearch(title));
				}

				string tags = queryResult.GetFieldValue("tags");
				if (!string.IsNullOrEmpty(tags))
				{
					martenQuery = martenQuery.Where(x => x.PlainTextSearch(tags));
				}

				string pageId = queryResult.GetFieldValue("pageId");
				if (!string.IsNullOrEmpty(pageId))
				{
					martenQuery = martenQuery.Where(x => x.PageId.PlainTextSearch(pageId));
				}

				string author = queryResult.GetFieldValue("author");
				if (!string.IsNullOrEmpty(pageId))
				{
					martenQuery = martenQuery.Where(x => x.Author.PlainTextSearch(author));
				}

				return await martenQuery.ToListAsync();
			}
		}
	}
}
