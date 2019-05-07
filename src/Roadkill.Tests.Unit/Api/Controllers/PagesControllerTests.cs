using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.Controllers;
using Roadkill.Api.ObjectConverters;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public class PagesControllerTests
	{
		private IPageRepository _pageRepositoryMock;
		private IPageObjectsConverter _viewObjectsConverterMock;
		private PagesController _pagesController;
		private Fixture _fixture;

		public PagesControllerTests()
		{
			_fixture = new Fixture();

			_pageRepositoryMock = Substitute.For<IPageRepository>();
			_viewObjectsConverterMock = Substitute.For<IPageObjectsConverter>();

			_viewObjectsConverterMock
				.ConvertToPageResponse(Arg.Any<Page>())
				.Returns(c =>
				{
					var page = c.Arg<Page>();
					return new PageResponse() { Id = page.Id, Title = page.Title };
				});

			_viewObjectsConverterMock
				.ConvertToPage(Arg.Any<PageRequest>())
				.Returns(c =>
				{
					var pageRequest = c.Arg<PageRequest>();
					return new Page()
					{
						Id = pageRequest.Id,
						Title = pageRequest.Title
					};
				});

			_pagesController = new PagesController(_pageRepositoryMock, _viewObjectsConverterMock);
		}

		[Theory]
		[InlineData(nameof(PagesController.Get), "{id}")]
		[InlineData(nameof(PagesController.AllPages))]
		[InlineData(nameof(PagesController.AllPagesCreatedBy))]
		[InlineData(nameof(PagesController.FindHomePage))]
		[InlineData(nameof(PagesController.FindByTitle))]
		public void Get_methods_should_be_HttpGet_with_custom_routeTemplate_and_allow_anonymous(string methodName, string routeTemplate = "")
		{
			Type attributeType = typeof(HttpGetAttribute);
			if (string.IsNullOrEmpty(routeTemplate))
			{
				routeTemplate = methodName;
			}

			_pagesController.ShouldHaveAttribute(methodName, attributeType);
			_pagesController.ShouldHaveRouteAttributeWithTemplate(methodName, routeTemplate);
			_pagesController.ShouldAllowAnonymous(methodName);
		}

		[Fact]
		public void Add_should_be_HttpPost_and_allow_editors()
		{
			string methodName = nameof(PagesController.Add);
			Type attributeType = typeof(HttpPostAttribute);

			_pagesController.ShouldHaveAttribute(methodName, attributeType);
			_pagesController.ShouldAuthorizeEditors(methodName);
		}

		[Fact]
		public void Update_should_be_HttpPut_and_allow_editors()
		{
			string methodName = nameof(PagesController.Update);
			Type attributeType = typeof(HttpPutAttribute);

			_pagesController.ShouldHaveAttribute(methodName, attributeType);
			_pagesController.ShouldAuthorizeEditors(methodName);
		}

		[Fact]
		public void Delete_should_be_HttpDelete_and_allow_admins()
		{
			string methodName = nameof(PagesController.Delete);
			Type attributeType = typeof(HttpDeleteAttribute);

			_pagesController.ShouldHaveAttribute(methodName, attributeType);
			_pagesController.ShouldAuthorizeAdmins(methodName);
		}

		[Fact]
		public async Task Add_should_call_repository_and_return_createdAtAction()
		{
			// given
			var pageRequest = _fixture.Create<PageRequest>();
			int autoIncrementedId = 99;

			_pageRepositoryMock
				.AddNewPageAsync(Arg.Any<Page>())
				.Returns(c =>
				{
					var page = c.Arg<Page>();

					// the repository returns a new id (autoincremented)
					return Task.FromResult(new Page()
					{
						Id = autoIncrementedId,
						Title = page.Title
					});
				});

			// when
			ActionResult<PageResponse> actionResult = await _pagesController.Add(pageRequest);

			// then
			await _pageRepositoryMock
				.Received(1)
				.AddNewPageAsync(Arg.Is<Page>(p => p.Title == pageRequest.Title));

			actionResult.ShouldBeCreatedAtActionResult();
			PageResponse pageResponse = actionResult.CreatedAtActionResultValue();
			pageResponse.ShouldNotBeNull("ActionResult's ViewModel was null");
			pageResponse.Id.ShouldBe(autoIncrementedId);
			pageResponse.Title.ShouldBe(pageRequest.Title);
		}

		[Fact]
		public async Task Update_should_update_using_repository_and_return_pageresponse()
		{
			// given
			var pageRequest = new PageRequest()
			{
				Id = 88,
				Title = "new title"
			};
			var changedPage = new Page()
			{
				Id = pageRequest.Id,
				Title = pageRequest.Title
			};
			var expectedResponse = new PageResponse()
			{
				Id = pageRequest.Id,
				Title = pageRequest.Title
			};

			_pageRepositoryMock
				.UpdateExistingAsync(Arg.Any<Page>())
				.Returns(changedPage);

			// when
			ActionResult<PageResponse> actionResult = await _pagesController.Update(pageRequest);

			// then
			await _pageRepositoryMock
				.Received(1)
				.UpdateExistingAsync(Arg.Is<Page>(page => page.Id == changedPage.Id));

			actionResult.Value.ShouldNotBeNull("ActionResult's model was null");
			actionResult.Value.ShouldBeEquivalent(expectedResponse);
		}

		[Fact]
		public async Task Delete_should_call_repository()
		{
			// given
			Page expectedPage = _fixture.Create<Page>();
			int expectedPageId = expectedPage.Id;

			_pageRepositoryMock
				.DeletePageAsync(expectedPageId)
				.Returns(Task.CompletedTask);

			// when
			await _pagesController.Delete(expectedPageId);

			// then
			await _pageRepositoryMock
				.Received(1)
				.DeletePageAsync(expectedPageId);
		}

		[Fact]
		public async Task Get_should_return_from_repository_and_call_converter()
		{
			// given
			Page expectedPage = _fixture.Create<Page>();
			int id = expectedPage.Id;

			_pageRepositoryMock
				.GetPageByIdAsync(id)
				.Returns(expectedPage);

			// when
			ActionResult<PageResponse> actionResult = await _pagesController.Get(id);

			// then
			actionResult.Value.ShouldNotBeNull("ActionResult's ViewModel was null");
			actionResult.Value.Id.ShouldBe(id);

			await _pageRepositoryMock
				.Received(1)
				.GetPageByIdAsync(id);

			_viewObjectsConverterMock
				.Received(1)
				.ConvertToPageResponse(expectedPage);
		}

		[Fact]
		public async Task AllPages_should_call_repository_and_converter()
		{
			// given
			IEnumerable<Page> pages = _fixture.CreateMany<Page>();

			_pageRepositoryMock
				.AllPagesAsync()
				.Returns(pages);

			// when
			ActionResult<IEnumerable<PageResponse>> actionResult = await _pagesController.AllPages();

			// then
			var model = (actionResult.Result as OkObjectResult).Value as IEnumerable<PageResponse>;

			model.ShouldNotBeNull("ActionResult's ViewModel was null");
			model.Count().ShouldBe(pages.Count());

			await _pageRepositoryMock
				.Received(1)
				.AllPagesAsync();

			_viewObjectsConverterMock
				.Received(pages.Count())
				.ConvertToPageResponse(Arg.Any<Page>());
		}

		[Fact]
		public async Task AllPagesCreatedBy_should_call_repository_and_converter()
		{
			// given
			string username = "bob";
			IEnumerable<Page> pages = _fixture.CreateMany<Page>();

			_pageRepositoryMock
				.FindPagesCreatedByAsync(username)
				.Returns(pages);

			// when
			ActionResult<IEnumerable<PageResponse>> actionResult = await _pagesController.AllPagesCreatedBy(username);

			// then
			var model = (actionResult.Result as OkObjectResult).Value as IEnumerable<PageResponse>;
			model.ShouldNotBeNull("ActionResult's ViewModel was null");
			model.Count().ShouldBe(pages.Count());

			await _pageRepositoryMock
				.Received(1)
				.FindPagesCreatedByAsync(username);

			_viewObjectsConverterMock
				.Received(pages.Count())
				.ConvertToPageResponse(Arg.Any<Page>());
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

			_pageRepositoryMock
				.FindPagesContainingTagAsync("homepage")
				.Returns(pages);

			// when
			ActionResult<PageResponse> actionResult = await _pagesController.FindHomePage();

			// then
			actionResult.Value.ShouldNotBeNull("ActionResult's ViewModel was null");
			actionResult.Value.Title.ShouldBe("page 0");
			actionResult.Value.Id.ShouldBe(pages[0].Id);

			await _pageRepositoryMock
				.Received(1)
				.FindPagesContainingTagAsync("homepage");

			_viewObjectsConverterMock
				.Received(1)
				.ConvertToPageResponse(pages[0]);
		}

		[Fact]
		public async Task FindByTitle()
		{
			// given
			Page expectedPage = _fixture.Create<Page>();
			string title = expectedPage.Title;

			_pageRepositoryMock
				.GetPageByTitleAsync(title)
				.Returns(expectedPage);

			// when
			ActionResult<PageResponse> actionResult = await _pagesController.FindByTitle(title);

			// then
			actionResult.Value.ShouldNotBeNull("ActionResult's ViewModel was null");
			actionResult.Value.Title.ShouldBe(title);
			actionResult.Value.Id.ShouldBe(expectedPage.Id);

			await _pageRepositoryMock
				.Received(1)
				.GetPageByTitleAsync(title);

			_viewObjectsConverterMock
				.Received(1)
				.ConvertToPageResponse(expectedPage);
		}
	}
}
