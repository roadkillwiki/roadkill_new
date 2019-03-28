using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Roadkill.Core.Entities;
using Roadkill.Core.Extensions;
using Roadkill.Core.Search.Adapters;
using Roadkill.Core.Settings;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Core.Adapters
{
	public class PostgresSearchAdapterTests
	{
		private readonly Fixture _fixture;
		private readonly ITestOutputHelper _outputHelper;

		private PostgresSearchAdapter _searchAdapter;
		private List<SearchablePage> _testPages;
		private IDocumentStore _documentStore;

		public PostgresSearchAdapterTests(ITestOutputHelper outputHelper)
		{
			_fixture = new Fixture();
			_testPages = new List<SearchablePage>();
			_outputHelper = outputHelper;

			string connectionString =
				"host=localhost;port=5432;database=roadkilltests;username=roadkill;password=roadkill;";

			var services = new ServiceCollection();
			services.AddLogging(builder => builder.AddXUnit(outputHelper));
			services.AddMartenDocumentStore(connectionString, new NullLogger<PostgresSearchAdapterTests>(), true);

			// Clean the postgres database
			var serviceProvider = services.BuildServiceProvider();
			_documentStore = serviceProvider.GetService<IDocumentStore>();
			_documentStore.Advanced.Clean.DeleteAllDocuments();

			_searchAdapter = new PostgresSearchAdapter(_documentStore);
		}

		[Fact]
		public async Task Add()
		{
			// given
			string title = _fixture.Create<string>();
			int id = (int)DateTime.Now.Ticks;
			var page = new SearchablePage() { PageId = id, Title = title };

			// when
			bool success = await _searchAdapter.Add(page);

			// then
			success.ShouldBeTrue();
		}

		[Fact(Skip = "TODO")]
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
			bool success = await _searchAdapter.Update(existingPage);

			// then
			success.ShouldBeTrue();

			var results = await _searchAdapter.Find($"{newTitle}");

			var firstResult = results.FirstOrDefault();
			firstResult.ShouldNotBeNull();
			firstResult.Title.ShouldBe(newTitle);
			firstResult.Author.ShouldBe(newAuthor);
			firstResult.Text.ShouldBe(newText);
		}

		[Theory(Skip = "TODO: parser")]
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
			IEnumerable<SearchablePage> results = await _searchAdapter.Find(query);

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
