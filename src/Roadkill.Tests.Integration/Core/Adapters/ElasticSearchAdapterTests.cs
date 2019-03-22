using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Elasticsearch.Net;
using Nest;
using Roadkill.Core.Adapters;
using Roadkill.Core.Entities;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Core.Adapters
{
	public class ElasticSearchAdapterTests : IClassFixture<ElasticSearchAdapterFixture>
	{
		private readonly Fixture _fixture;
		private readonly ITestOutputHelper _console;
		private readonly ElasticSearchAdapterFixture _classFixture;

		private readonly ElasticSearchAdapter _elasticSearchAdapter;
		private readonly List<SearchablePage> _testPages;

/*
		These tests need ElasticSearch installed locally, you can do this
		by running the ElasticSearch Docker image:

		docker run -d -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" docker.elastic.co/elasticsearch/elasticsearch:6.2.4

		~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		More details:

		https://www.docker.elastic.co/
		https://www.elastic.co/guide/en/elasticsearch/reference/6.2/docker.html
		https://github.com/elastic/elasticsearch-net

		RESTful api examples:
		https://github.com/sittinash/elasticsearch-postman
		http://{{url}}:{{port}}/pages/_search?pretty=true&q=*:*
		http://localhost:9200/pages/_search?pretty=true&q=*:*
		GET /_cat/indices?v
*/

		public ElasticSearchAdapterTests(ITestOutputHelper console, ElasticSearchAdapterFixture classFixture)
		{
			_fixture = new Fixture();
			_console = console;
			_classFixture = classFixture;

			var uri = new Uri("http://localhost:9200");
			var connectionPool = new StaticConnectionPool(new List<Node>() { uri });
			var connectionSettings = new ConnectionSettings(connectionPool);
			var elasticClient = new ElasticClient(connectionSettings);
			_elasticSearchAdapter = new ElasticSearchAdapter(elasticClient);
			_testPages = classFixture.TestPages;
		}

		[Fact]
		public async Task Add()
		{
			// given
			string title = _fixture.Create<string>();
			int id = (int)DateTime.Now.Ticks;
			var page = new SearchablePage() { Id = id, Title = title };

			// when
			bool success = await _elasticSearchAdapter.Add(page);

			// then
			success.ShouldBeTrue();

			long count = _classFixture.ElasticClient.Count<SearchablePage>().Count;
			count.ShouldBe(11);
			var results = await _elasticSearchAdapter.Find($"{title}");
			var firstResult = results.FirstOrDefault();
			firstResult.ShouldNotBeNull();
			firstResult.Title.ShouldBe(title);
			firstResult.Id.ShouldBe(page.Id);
		}

		[Fact]
		public async Task Update()
		{
			// given
			var existingPage = _testPages.First();

			string newTitle = _fixture.Create<string>();
			string newText = _fixture.Create<string>();
			string newAuthor = _fixture.Create<string>();

			existingPage.Title = newTitle;
			existingPage.Author = newAuthor;
			existingPage.Text = newText;

			// when
			bool success = await _elasticSearchAdapter.Update(existingPage);

			// then
			success.ShouldBeTrue();

			var results = await _elasticSearchAdapter.Find($"{newTitle}");

			var firstResult = results.FirstOrDefault();
			firstResult.ShouldNotBeNull();
			firstResult.Title.ShouldBe(newTitle);
			firstResult.Author.ShouldBe(newAuthor);
			firstResult.Text.ShouldBe(newText);
		}

		[Theory(Skip = "Currently flakey")]
		[InlineData("Id", "id:{0}")]
		[InlineData("Title", "title:{0}")]
		[InlineData("Text", "text:{0}")]
		[InlineData("Tags", "tags:{0}")]
		[InlineData("Author", "author:{0}")]
		public async Task Find(string property, string query)
		{
			// given
			var page = _testPages.First();

			var propertyValue = typeof(SearchablePage)
									.GetProperty(property)
									.GetValue(page, null);
			query = string.Format(query, propertyValue);

			// when
			IEnumerable<SearchablePage> results = await _elasticSearchAdapter.Find(query);

			// then
			var firstResult = results.FirstOrDefault();
			firstResult.ShouldNotBeNull();
			firstResult.Id.ShouldBe(page.Id);
			firstResult.Text.ShouldBe(page.Text);
			firstResult.Title.ShouldBe(page.Title);
			firstResult.Tags.ShouldBe(page.Tags);
			firstResult.Author.ShouldBe(page.Author);
			firstResult.DateTime.ShouldBe(page.DateTime);
		}
	}
}
