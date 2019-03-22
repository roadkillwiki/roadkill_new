using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Marten;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Core.Repositories
{
	public class PageVersionRepositoryTests
	{
		private readonly Fixture _fixture;
		private readonly ITestOutputHelper _outputHelper;

		public PageVersionRepositoryTests(ITestOutputHelper outputHelper)
		{
			_fixture = new Fixture();
			_outputHelper = outputHelper;
			IDocumentStore documentStore = DocumentStoreManager.GetMartenDocumentStore(typeof(PageVersionRepository), outputHelper);

			try
			{
				new PageRepository(documentStore).Wipe();
				new PageVersionRepository(documentStore).Wipe();
			}
			catch (Exception e)
			{
				outputHelper.WriteLine(GetType().Name + " caught: " + e.Message);
			}
		}

		public PageVersionRepository CreateRepository()
		{
			IDocumentStore documentStore = DocumentStoreManager.GetMartenDocumentStore(typeof(PageVersionRepository), _outputHelper);

			return new PageVersionRepository(documentStore);
		}

		[Fact]
		public async void AddNewVersion()
		{
			// given
			PageVersionRepository repository = CreateRepository();
			List<PageVersion> pages = CreateTenPages(repository);
			PageVersion expectedPage = pages.Last();
			await repository.AddNewVersion(expectedPage.PageId, "v2 text", "brian");

			// when
			PageVersion thirdVersion = await repository.AddNewVersion(expectedPage.PageId, "v3 text", "author2");

			// then
			thirdVersion.ShouldNotBeNull();

			PageVersion savedVersion = await repository.GetById(thirdVersion.Id);
			savedVersion.ShouldNotBeNull();
			savedVersion.ShouldBeEquivalent(thirdVersion);
		}

		[Fact]
		public async void AllVersions()
		{
			// given
			PageVersionRepository repository = CreateRepository();
			List<PageVersion> pages = CreateTenPages(repository);

			// when
			IEnumerable<PageVersion> allVersions = await repository.AllVersions();

			// then
			allVersions.Count().ShouldBe(pages.Count);
			allVersions.Last().Text.ShouldNotBeEmpty();
		}

		[Fact]
		public async void DeleteVersion()
		{
			// given
			PageVersionRepository repository = CreateRepository();
			List<PageVersion> pages = CreateTenPages(repository);

			var expectedPage = pages[0];
			var version2 = await repository.AddNewVersion(expectedPage.PageId, "v2", "author2");
			var version3 = await repository.AddNewVersion(expectedPage.PageId, "v3", "author2");

			// when
			await repository.DeleteVersion(version3.Id);

			// then
			var deletedVersion = await repository.GetById(version3.Id);
			deletedVersion.ShouldBeNull();

			var latestVersion = await repository.GetById(version2.Id);
			latestVersion.ShouldNotBeNull();
		}

		[Fact]
		public async Task UpdateExistingVersion()
		{
			// given
			PageVersionRepository repository = CreateRepository();
			List<PageVersion> pageVersions = CreateTenPages(repository);

			PageVersion newVersion = pageVersions[0];
			newVersion.Text = "some new text";
			newVersion.Author = "blake";

			// when
			await repository.UpdateExistingVersion(newVersion);

			// then
			PageVersion savedVersion = await repository.GetById(newVersion.Id);
			savedVersion.ShouldNotBeNull();
			savedVersion.ShouldBeEquivalent(newVersion);
		}

		[Fact]
		public async Task FindPageVersionsByPageId_should_return_versions_sorted_by_date_desc()
		{
			// given
			PageVersionRepository repository = CreateRepository();
			List<PageVersion> pages = CreateTenPages(repository);

			var firstPage = pages[0];
			var version2 = await repository.AddNewVersion(firstPage.PageId, "v2", "author1", DateTime.Today.AddMinutes(10));
			var version3 = await repository.AddNewVersion(firstPage.PageId, "v3", "author2", DateTime.Today.AddMinutes(20));
			var version4 = await repository.AddNewVersion(firstPage.PageId, "v4", "author3", DateTime.Today.AddMinutes(30));

			// when
			IEnumerable<PageVersion> versions = await repository.FindPageVersionsByPageId(firstPage.PageId);

			// then
			versions.ShouldNotBeNull();
			versions.ShouldNotBeEmpty();
			versions.Count().ShouldBe(4);
			versions.First().ShouldBeEquivalent(version4);
			versions.Last().ShouldBeEquivalent(firstPage);
		}

		[Fact]
		public async Task FindPageVersionsByAuthor_should_be_case_insensitive_and_return_versions_sorted_by_date_desc()
		{
			// given
			PageVersionRepository repository = CreateRepository();
			List<PageVersion> pageVersions = CreateTenPages(repository);

			string editedBy = "shakespeare jr";
			PageVersion version2 = await repository.AddNewVersion(pageVersions[0].PageId, "v2 text", editedBy);
			PageVersion version3 = await repository.AddNewVersion(pageVersions[1].PageId, "v3 text", editedBy);

			// when
			IEnumerable<PageVersion> actualPageVersions = await repository.FindPageVersionsByAuthor("SHAKESPEARE jr");

			// then
			actualPageVersions.Count().ShouldBe(2);
			actualPageVersions.ShouldContain(p => p.Id == version2.Id);
			actualPageVersions.ShouldContain(p => p.Id == version3.Id);
		}

		[Fact]
		public async Task GetLatestVersions()
		{
			// given
			PageVersionRepository repository = CreateRepository();
			List<PageVersion> pageVersions = CreateTenPages(repository);

			int pageId = pageVersions[0].PageId;
			PageVersion version2 = await repository.AddNewVersion(pageId, "v2 text", "editedBy", DateTime.Today.AddMinutes(10));
			PageVersion version3 = await repository.AddNewVersion(pageId, "v3 text", "editedBy", DateTime.Today.AddMinutes(30));

			// when
			PageVersion latestVersion = await repository.GetLatestVersion(pageId);

			// then
			latestVersion.ShouldNotBeNull();
			latestVersion.ShouldBeEquivalent(version3);
		}

		[Fact]
		public async Task GetById()
		{
			// given
			PageVersionRepository repository = CreateRepository();
			List<PageVersion> pageVersions = CreateTenPages(repository);
			PageVersion pageVersion = pageVersions[0];

			// when
			PageVersion latestVersion = await repository.GetById(pageVersion.Id);

			// then
			Assert.NotNull(latestVersion);
			latestVersion.ShouldNotBeNull();
			latestVersion.ShouldBeEquivalent(pageVersion);
		}

		private List<PageVersion> CreateTenPages(PageVersionRepository repository)
		{
			IDocumentStore documentStore = DocumentStoreManager.GetMartenDocumentStore(typeof(PageVersionRepository), _outputHelper);
			var pageRepository = new PageRepository(documentStore);

			List<Page> pages = _fixture.CreateMany<Page>(10).ToList();

			var pageVersions = new List<PageVersion>();
			foreach (Page page in pages)
			{
				string text = _fixture.Create<string>();
				string author = _fixture.Create<string>();
				DateTime dateTime = DateTime.Today;

				Page newPage = pageRepository.AddNewPage(page).GetAwaiter().GetResult();
				PageVersion pageVersion = repository.AddNewVersion(newPage.Id, text, author, dateTime).GetAwaiter().GetResult();
				pageVersions.Add(pageVersion);
			}

			return pageVersions;
		}
	}
}
