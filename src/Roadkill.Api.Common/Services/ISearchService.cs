using System.Collections.Generic;
using System.Threading.Tasks;
using Roadkill.Api.Common.Models;

namespace Roadkill.Api.Common.Services
{
	public interface ISearchService
	{
		Task<IEnumerable<SearchResponseModel>> Search(string searchText);

		Task<string> Add(SearchRequestModel searchRequest);

		int Delete(int id);

		void Update(SearchRequestModel searchRequest);
	}
}
