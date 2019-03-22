using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using Elasticsearch.Net;
using Nest;
using Roadkill.Core.Adapters;
using Roadkill.Core.Entities;

namespace Roadkill.Tests.Integration.Core.Adapters
{
	[SuppressMessage("ReSharper", "CA1063", Justification = "Dispose rules don't apply in test fixtures")]
	[SuppressMessage("ReSharper", "CA1816", Justification = "Dispose rules don't apply in test fixtures")]
	public class ElasticSearchAdapterFixture : IDisposable
	{
		public ElasticSearchAdapterFixture()
		{
			var uri = new Uri("http://localhost:9200");
			var connectionPool = new SingleNodeConnectionPool(uri);
			var connectionSettings = new ConnectionSettings(connectionPool);
			ElasticClient = new ElasticClient(connectionSettings);
			ElasticSearchAdapter = new ElasticSearchAdapter(ElasticClient);

			AddDummyData();
		}

		public List<SearchablePage> TestPages { get; private set; }

		public ElasticClient ElasticClient { get; set; }

		public ElasticSearchAdapter ElasticSearchAdapter { get; set; }

		public void Dispose()
		{
			ElasticClient.DeleteIndex(ElasticSearchAdapter.PagesIndexName);
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
	}
}
