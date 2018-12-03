using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoFixture;
using Elasticsearch.Net;
using Nest;
using Roadkill.Core.Adapters;
using Roadkill.Core.Models;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Adapters
{
	public class ElasticSearchAdapterFixture : IDisposable
	{
		public List<SearchablePage> TestPages { get; set; }
		public ElasticClient ElasticClient { get; set; }
		public ElasticSearchAdapter ElasticSearchAdapter { get; set; }

		public ElasticSearchAdapterFixture()
		{
			var uri = new Uri("http://localhost:9200");
			var connectionPool = new SingleNodeConnectionPool(uri);
			var connectionSettings = new ConnectionSettings(connectionPool);
			ElasticClient = new ElasticClient(connectionSettings);
			ElasticSearchAdapter = new ElasticSearchAdapter(ElasticClient);

			AddDummyData();
		}

		private void AddDummyData()
		{
			var fixture = new Fixture();

			int id = (int)DateTime.Now.Ticks;
			TestPages = fixture.CreateMany<SearchablePage>(10).ToList();
			foreach (SearchablePage page in TestPages)
			{
				page.Id = ++id;
				ElasticSearchAdapter.Add(page).ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}

		public void Dispose()
		{
			ElasticClient.DeleteIndex(ElasticSearchAdapter.PagesIndexName);
		}
	}
}