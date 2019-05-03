using System.Collections.Generic;
using System.Threading.Tasks;
using Roadkill.Api.Common.Models;

namespace Roadkill.Api.Client
{
	public interface ISearchClient
	{
		Task<IEnumerable<SearchResponseModel>> Search(string searchText);

		Task<string> Add(SearchRequestModel searchRequest);

		int Delete(int id);

		void Update(SearchRequestModel searchRequest);
	}
}
