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
			await EnsureIndexesExist();
			var response = await _elasticClient.IndexAsync(page, idx => idx.Index(PagesIndexName));

			return response.Result == Result.Created;
		}

		public async Task RecreateIndex()
		{
			await _elasticClient.DeleteIndexAsync(PagesIndexName);
			await EnsureIndexesExist();
		}

		public async Task<bool> Update(SearchablePage page)
		{
			await EnsureIndexesExist();
			var response = await _elasticClient.IndexAsync(page, idx => idx.Index(PagesIndexName));

			return response.Result == Result.Updated;
		}

		public async Task<IEnumerable<SearchablePage>> Find(string query)
		{
			await EnsureIndexesExist();

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

		private async Task EnsureIndexesExist()
		{
			var existsResponse = await _elasticClient.IndexExistsAsync(PagesIndexName);
			if (!existsResponse.Exists)
			{
				await _elasticClient.CreateIndexAsync(PagesIndexName);
			}
		}
	}
}
