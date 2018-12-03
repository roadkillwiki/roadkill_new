using System.Collections.Generic;
using System.Threading.Tasks;
using Roadkill.Api.Models;
using Roadkill.Core.Models;

namespace Roadkill.Api.Interfaces
{
	public interface ISearchService
	{
		Task<IEnumerable<SearchResponseModel>> Search(string searchText);

		Task<string> Add(SearchRequestModel searchRequest);

		int Delete(int id);

		void Update(SearchRequestModel searchRequest);
	}
}