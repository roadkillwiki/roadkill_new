using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using Roadkill.Core.Entities;

namespace Roadkill.Core.Search.Adapters
{
	public class ElasticSearchAdapter : ISearchAdapter
	{
		public const string PagesIndexName = "pages";
		private readonly IElasticClient _elasticClient;

		public ElasticSearchAdapter(IElasticClient elasticClient)
		{
			_elasticClient = elasticClient;
		}

		public async Task<bool> Add(SearchablePage page)
		{
			var response = await _elasticClient.IndexAsync(page, idx => idx.Index(PagesIndexName));

			return response.Result == Result.Created;
		}

		public async Task RecreateIndex()
		{
			_elasticClient.LowLevel.Indices.Delete<SearchablePageResponse>(ElasticSearchAdapter.PagesIndexName);
			await _elasticClient.ReindexOnServerAsync(descriptor => descriptor);
		}

		public async Task<bool> Update(SearchablePage page)
		{
			var response = await _elasticClient.IndexAsync(page, idx => idx.Index(PagesIndexName));
			return response.Result == Result.Updated;
		}

		public async Task<IEnumerable<SearchablePage>> Find(string query)
		{
			var searchDescriptor = CreateSearchDescriptor(query);
			var response = await _elasticClient.SearchAsync<SearchablePage>(searchDescriptor);

			return response.Documents.AsEnumerable();
		}

		private static SearchDescriptor<SearchablePage> CreateSearchDescriptor(string query)
		{
			return new SearchDescriptor<SearchablePage>()
				.From(0)
				.Size(20)
				.Index(PagesIndexName)
				.Query(q => q.QueryString(qs => qs.Query(query)));
		}
	}
}
