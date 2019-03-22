using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Marten;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Core.Repositories
{
	public class DummyDataMakerTests
	{
		private readonly Fixture _fixture;
		private readonly ITestOutputHelper _outputHelper;

		public DummyDataMakerTests(ITestOutputHelper outputHelper)
		{
			_fixture = new Fixture();
			_outputHelper = outputHelper;
		}

		private PageRepository CreateRepository()
		{
			IDocumentStore documentStore = DocumentStoreManager.GetMartenDocumentStore(null, _outputHelper);
			return new PageRepository(documentStore);
		}

		private List<Page> CreateTenPages(IPageRepository repository, List<Page> pages = null)
		{
			if (pages == null)
			{
				pages = _fixture.CreateMany<Page>(10).ToList();
			}

			var newPages = new List<Page>();
			foreach (Page page in pages)
			{
				page.Title += " some kind of £ encoding is <needed> for this src=\"i think\"";
				Page newPage = repository.AddNewPage(page).GetAwaiter().GetResult();
				newPages.Add(newPage);
			}

			return newPages;
		}

		private void TenPagesPlease()
		{
			PageRepository repository = CreateRepository();
			CreateTenPages(repository);
		}
	}
}
