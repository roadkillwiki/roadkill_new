using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Authorization;
using Roadkill.Api.Authorization.Roles;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Core.Search.Adapters;

namespace Roadkill.Api.Controllers
{
	[Route("[controller]")]
	[AuthorizeWithBearer]
	public class SearchController : ControllerBase
	{
		private readonly ISearchAdapter _searchAdapter;

		public SearchController(ISearchAdapter searchAdapter)
		{
			_searchAdapter = searchAdapter;
		}

		[HttpGet]
		[Route(nameof(Search))]
		public Task<IEnumerable<SearchResponse>> Search(string searchText)
		{
			throw new NotImplementedException();
		}

		[HttpPost]
		[Route(nameof(Add))]
		public Task<string> Add(SearchRequest searchRequest)
		{
			throw new NotImplementedException();
		}

		[HttpDelete]
		[Route(nameof(Delete))]
		public int Delete(int id)
		{
			throw new NotImplementedException();
		}

		[HttpPut]
		[Route(nameof(Update))]
		public void Update(SearchRequest searchRequest)
		{
			throw new NotImplementedException();
		}
	}
}
