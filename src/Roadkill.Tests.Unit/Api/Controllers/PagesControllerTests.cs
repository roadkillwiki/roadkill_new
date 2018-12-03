using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Roadkill.Api.Controllers;
using Roadkill.Api.Models;
using Roadkill.Core.Models;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;

// ReSharper disable PossibleMultipleEnumeration

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public class PagesControllerTests
	{
		private Mock<IPageRepository> _pageRepositoryMock;
		private Mock<IPageModelConverter> _viewModelCreatorMock;
		private PagesController _pagesController;
		private Fixture _fixture;

		public PagesControllerTests()
		{
			_fixture = new Fixture();

			_pageRepositoryMock = new Mock<IPageRepository>();

			_viewModelCreatorMock = new Mock<IPageModelConverter>();

			_viewModelCreatorMock
				.Setup(x => x.ConvertToViewModel(It.IsAny<Page>()))
				.Returns<Page>(page => new PageModel() { Id = page.Id, Title = page.Title });

			_viewModelCreatorMock
				.Setup(x => x.ConvertToPage(It.IsAny<PageModel>()))
				.Returns<PageModel>(viewModel => new Page() { Id = viewModel.Id, Title = viewModel.Title });

			_pagesController = new PagesController(_pageRepositoryMock.Object, _viewModelCreatorMock.Object);
		}

		[Fact]
		public void restful_verb_methods_should_have_httpattributes()
		{
			_pagesController.ShouldHaveAttribute(nameof(PagesController.Add), typeof(HttpPostAttribute));
			_pagesController.ShouldHaveAttribute(nameof(PagesController.Update), typeof(HttpPutAttribute));
			_pagesController.ShouldHaveAttribute(nameof(PagesController.Delete), typeof(HttpDeleteAttribute));
			_pagesController.ShouldHaveAttribute(nameof(PagesController.GetById), typeof(HttpGetAttribute));
		}

		[Fact]
		public async Task Add()
		{
			// given
			var inputPageViewModel = _fixture.Create<PageModel>();
			int autoIncrementedId = 99;

			_pageRepositoryMock
				.Setup(x => x.AddNewPage(It.IsAny<Page>()))
				.Returns<Page>(page =>
				{
					// the repository returns a new id (autoincremented)
					return Task.FromResult(new Page()
					{
						Id = autoIncrementedId,
						Title = page.Title
					});
				});

			// when
			var actualPageViewModel = await _pagesController.Add(inputPageViewModel);

			// then
			actualPageViewModel.Id.ShouldBe(autoIncrementedId);
			actualPageViewModel.Title.ShouldBe(inputPageViewModel.Title);

			_pageRepositoryMock
				.Verify(x => x.AddNewPage(It.Is<Page>(p => p.Id == inputPageViewModel.Id)), Times.Once);
		}

		[Fact]
		public async Task Update()
		{
			// given
			var changedPageViewModel = new PageModel()
			{
				Id = 88,
				Title = "new title"
			};
			var changedPage = new Page()
			{
				Id = changedPageViewModel.Id,
				Title = changedPageViewModel.Title
			};

			_pageRepositoryMock
				.Setup(x => x.UpdateExisting(It.IsAny<Page>()))
				.ReturnsAsync(changedPage);

			// when
			PageModel actualChangedPage = await _pagesController.Update(changedPageViewModel);

			// then
			actualChangedPage.ShouldBeEquivalent(changedPageViewModel);
			_pageRepositoryMock.Verify(x => x.UpdateExisting(It.Is<Page>(page => page.Id == changedPage.Id)), Times.Once);
		}

		[Fact]
		public async Task Delete()
		{
			// given
			Page expectedPage = _fixture.Create<Page>();
			int expectedPageId = expectedPage.Id;

			_pageRepositoryMock
				.Setup(x => x.DeletePage(expectedPageId))
				.Returns(Task.CompletedTask);

			// when
			await _pagesController.Delete(expectedPageId);

			// then
			_pageRepositoryMock.Verify(x => x.DeletePage(expectedPageId), Times.Once);
		}

		[Fact]
		public async Task GetById()
		{
			// given
			Page expectedPage = _fixture.Create<Page>();
			int id = expectedPage.Id;

			_pageRepositoryMock
				.Setup(x => x.GetPageById(id))
				.ReturnsAsync(expectedPage);

			// when
			PageModel pageModel = await _pagesController.GetById(id);

			// then
			pageModel.ShouldNotBeNull();
			pageModel.Id.ShouldBe(id);

			_pageRepositoryMock.Verify(x => x.GetPageById(id), Times.Once);
			_viewModelCreatorMock.Verify(x => x.ConvertToViewModel(expectedPage));
		}

		[Fact]
		public async Task AllPages_should_call_repository_and_converter()
		{
			// given
			IEnumerable<Page> pages = _fixture.CreateMany<Page>();

			_pageRepositoryMock
				.Setup(x => x.AllPages())
				.ReturnsAsync(pages);

			// when
			IEnumerable<PageModel> pageViewModels = await _pagesController.AllPages();

			// then
			pageViewModels.Count().ShouldBe(pages.Count());

			_pageRepositoryMock.Verify(x => x.AllPages(), Times.Once);
			_viewModelCreatorMock.Verify(x => x.ConvertToViewModel(It.IsAny<Page>()));
		}

		[Fact]
		public async Task AllPagesCreatedBy_should_call_repository_and_converter()
		{
			// given
			string username = "bob";
			IEnumerable<Page> pages = _fixture.CreateMany<Page>();

			_pageRepositoryMock
				.Setup(x => x.FindPagesCreatedBy(username))
				.ReturnsAsync(pages);

			// when
			IEnumerable<PageModel> pageViewModels = await _pagesController.AllPagesCreatedBy(username);

			// then
			pageViewModels.Count().ShouldBe(pages.Count());

			_pageRepositoryMock.Verify(x => x.FindPagesCreatedBy(username), Times.Once);
			_viewModelCreatorMock.Verify(x => x.ConvertToViewModel(It.IsAny<Page>()));
		}

		[Fact]
		public async Task FindHomePage_should_return_first_page_with_homepage_tag()
		{
			// given
			List<Page> pages = _fixture.CreateMany<Page>(10).ToList();
			for (int i = 0; i < pages.Count; i++)
			{
				pages[i].Tags += ", homepage";
				pages[i].Title = $"page {i}";
			}

			_pageRepositoryMock.Setup(x => x.FindPagesContainingTag("homepage"))
				.ReturnsAsync(pages);

			// when
			PageModel pageModel = await _pagesController.FindHomePage();

			// then
			pageModel.ShouldNotBeNull();
			pageModel.Title.ShouldBe("page 0");
			pageModel.Id.ShouldBe(pages[0].Id);

			_pageRepositoryMock.Verify(x => x.FindPagesContainingTag("homepage"), Times.Once);
			_viewModelCreatorMock.Verify(x => x.ConvertToViewModel(pages[0]));
		}

		[Fact]
		public async Task FindByTitle()
		{
			// given
			Page expectedPage = _fixture.Create<Page>();
			string title = expectedPage.Title;

			_pageRepositoryMock.Setup(x => x.GetPageByTitle(title))
				.ReturnsAsync(expectedPage);

			// when
			PageModel pageModel = await _pagesController.FindByTitle(title);

			// then
			pageModel.ShouldNotBeNull();
			pageModel.Title.ShouldBe(title);
			pageModel.Id.ShouldBe(expectedPage.Id);

			_pageRepositoryMock.Verify(x => x.GetPageByTitle(title), Times.Once);
			_viewModelCreatorMock.Verify(x => x.ConvertToViewModel(expectedPage));
		}
	}
}