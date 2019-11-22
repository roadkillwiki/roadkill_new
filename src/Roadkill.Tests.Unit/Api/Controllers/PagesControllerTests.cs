using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Roadkill.Api.Authorization;
using Roadkill.Api.Authorization.Policies;
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
		public void Add_should_be_HttpPost_and_authorize_policy()
		{
			string methodName = nameof(PagesController.Add);
			Type attributeType = typeof(HttpPostAttribute);

			_pagesController.ShouldHaveAttribute(methodName, attributeType);
			_pagesController.ShouldAuthorizePolicy(methodName, PolicyNames.AddPage);
		}

		[Fact]
		public void Update_should_be_HttpPut_and_authorize_policy()
		{
			string methodName = nameof(PagesController.Update);
			Type attributeType = typeof(HttpPutAttribute);

			_pagesController.ShouldHaveAttribute(methodName, attributeType);
			_pagesController.ShouldAuthorizePolicy(methodName, PolicyNames.UpdatePage);
		}

		[Fact]
		public void Delete_should_be_HttpDelete_and_authorize_policy()
		{
			string methodName = nameof(PagesController.Delete);
			Type attributeType = typeof(HttpDeleteAttribute);

			_pagesController.ShouldHaveAttribute(methodName, attributeType);
			_pagesController.ShouldAuthorizePolicy(methodName, PolicyNames.DeletePage);
		}

		[Fact]
		public async Task Get_should_return_page()
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
		}

		[Fact]
		public async Task AllPages_should_return_pages()
		{
			// given
			IEnumerable<Page> pages = _fixture.CreateMany<Page>();

			_pageRepositoryMock
				.AllPagesAsync()
				.Returns(pages);

			// when
			ActionResult<IEnumerable<PageResponse>> actionResult = await _pagesController.AllPages();

			// then
			actionResult.ShouldBeOkObjectResult();
			IEnumerable<PageResponse> actualPage = actionResult.GetOkObjectResultValue();
			actualPage.Count().ShouldBe(pages.Count());
		}

		[Fact]
		public async Task AllPagesCreatedBy_should_return_pages_for_user()
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
			actionResult.ShouldBeOkObjectResult();
			IEnumerable<PageResponse> actualPage = actionResult.GetOkObjectResultValue();
			actualPage.Count().ShouldBe(pages.Count());
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
			PageResponse actualPage = actionResult.GetOkObjectResultValue();
			actualPage.Title.ShouldBe("page 0");
			actualPage.Id.ShouldBe(pages[0].Id);
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
			PageResponse actualPage = actionResult.GetOkObjectResultValue();
			actualPage.Title.ShouldBe(title);
			actualPage.Id.ShouldBe(expectedPage.Id);
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
			actionResult.ShouldBeCreatedAtActionResult();
			PageResponse pageResponse = actionResult.CreatedAtActionResultValue();
			pageResponse.ShouldNotBeNull("ActionResult's ViewModel was null");
			pageResponse.Id.ShouldBe(autoIncrementedId);
			pageResponse.Title.ShouldBe(pageRequest.Title);
		}

		[Fact]
		public async Task Update_should_return_pageresponse()
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
			actionResult.ShouldBeNoContentResult();
		}

		[Fact]
		public async Task Delete_should_return_no_content()
		{
			// given
			Page expectedPage = _fixture.Create<Page>();
			int expectedPageId = expectedPage.Id;

			_pageRepositoryMock
				.DeletePageAsync(expectedPageId)
				.Returns(Task.CompletedTask);

			// when
			ActionResult<string> actionResult = await _pagesController.Delete(expectedPageId);

			// then
			actionResult.ShouldBeNoContentResult();
		}
	}
}
