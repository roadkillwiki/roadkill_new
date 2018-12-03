using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Roadkill.Api.Interfaces;
using Roadkill.Api.Models;
using Roadkill.Core.Adapters;

namespace Roadkill.Api.Controllers
{
	public class SearchController : Controller, ISearchService
	{
		private readonly IElasticSearchAdapter _searchAdapter;

		public SearchController(IElasticSearchAdapter searchAdapter)
		{
			_searchAdapter = searchAdapter;
		}

		[HttpGet]
		[Route(nameof(Search))]
		public Task<IEnumerable<SearchResponseModel>> Search(string searchText)
		{
			throw new NotImplementedException();
		}

		[HttpPost]
		[Route(nameof(Add))]
		public Task<string> Add(SearchRequestModel searchRequest)
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
		public void Update(SearchRequestModel searchRequest)
		{
			throw new NotImplementedException();
		}
	}
}