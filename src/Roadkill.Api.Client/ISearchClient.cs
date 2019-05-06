using System.Collections.Generic;
using System.Threading.Tasks;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;

namespace Roadkill.Api.Client
{
	public interface ISearchClient
	{
		Task<IEnumerable<SearchResponse>> Search(string searchText);

		Task<string> Add(SearchRequest searchRequest);

		int Delete(int id);

		void Update(SearchRequest searchRequest);
	}
}
