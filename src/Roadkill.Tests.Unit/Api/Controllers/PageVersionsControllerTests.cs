using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Roadkill.Api.Authorization;
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
	public class PageVersionsControllerTests
	{
		private readonly Fixture _fixture;
		private readonly IPageVersionObjectsConverter _objectsConverterMock;
		private readonly IPageVersionRepository _pageVersionRepositoryMock;
		private PageVersionsController _pageVersionsController;

		public PageVersionsControllerTests()
		{
			_fixture = new Fixture();

			_pageVersionRepositoryMock = Substitute.For<IPageVersionRepository>();

			_objectsConverterMock = Substitute.For<IPageVersionObjectsConverter>();
			_objectsConverterMock
				.ConvertToPageVersionResponse(Arg.Any<PageVersion>())
				.Returns(callInfo =>
				{
					var pageVersion = callInfo.Arg<PageVersion>();

					return new PageVersionResponse()
					{
						Id = pageVersion.Id,
						Text = pageVersion.Text,
						Author = pageVersion.Author,
						DateTime = pageVersion.DateTime,
						PageId = pageVersion.PageId
					};
				});

			_pageVersionsController = new PageVersionsController(_pageVersionRepositoryMock, _objectsConverterMock);
		}

		[Theory]
		[InlineData(nameof(PageVersionsController.Get), "{id}")]
		[InlineData(nameof(PageVersionsController.AllVersions))]
		[InlineData(nameof(PageVersionsController.FindPageVersionsByPageId))]
		[InlineData(nameof(PageVersionsController.FindPageVersionsByAuthor))]
		[InlineData(nameof(PageVersionsController.GetLatestVersion))]
		public void Get_methods_should_be_HttpGet_with_custom_routeTemplate_and_allow_anonymous(string methodName, string routeTemplate = "")
		{
			Type attributeType = typeof(HttpGetAttribute);
			if (string.IsNullOrEmpty(routeTemplate))
			{
				routeTemplate = methodName;
			}

			_pageVersionsController.ShouldHaveAttribute(methodName, attributeType);
			_pageVersionsController.ShouldHaveRouteAttributeWithTemplate(methodName, routeTemplate);
			_pageVersionsController.ShouldAllowAnonymous(methodName);
		}

		[Fact]
		public void Add_should_be_HttpPost_and_allow_authorizepolicy()
		{
			string methodName = nameof(PageVersionsController.Add);
			Type attributeType = typeof(HttpPostAttribute);

			_pageVersionsController.ShouldHaveAttribute(methodName, attributeType);
			_pageVersionsController.ShouldAuthorizePolicy(methodName, PolicyNames.AddPage);
		}

		[Fact]
		public void Update_should_be_HttpPut_and_allow_authorizepolicy()
		{
			string methodName = nameof(PageVersionsController.Update);
			Type attributeType = typeof(HttpPutAttribute);

			_pageVersionsController.ShouldHaveAttribute(methodName, attributeType);
			_pageVersionsController.ShouldAuthorizePolicy(methodName, PolicyNames.UpdatePage);
		}

		[Fact]
		public void Delete_should_be_HttpDelete_and_authorize_policy()
		{
			string methodName = nameof(PageVersionsController.Delete);
			Type attributeType = typeof(HttpDeleteAttribute);

			_pageVersionsController.ShouldHaveAttribute(methodName, attributeType);
			_pageVersionsController.ShouldAuthorizePolicy(methodName, PolicyNames.DeletePage);
		}

		[Fact]
		public async Task GetById()
		{
			// given
			PageVersion pageVersion = _fixture.Create<PageVersion>();
			Guid versionId = pageVersion.Id;

			_pageVersionRepositoryMock
				.GetByIdAsync(versionId)
				.Returns(pageVersion);

			// when
			ActionResult<PageVersionResponse> actionResult = await _pageVersionsController.Get(versionId);

			// then
			actionResult.ShouldBeOkObjectResult();

			PageVersionResponse response = actionResult.GetOkObjectResultValue();
			response.ShouldNotBeNull();
			response.Id.ShouldBe(versionId);
		}

		[Fact]
		public async Task GetLatestVersion()
		{
			// given
			PageVersion pageVersion = _fixture.Create<PageVersion>();
			int pageId = pageVersion.PageId;

			_pageVersionRepositoryMock
				.GetLatestVersionAsync(pageId)
				.Returns(pageVersion);

			// when
			ActionResult<PageVersionResponse> actionResult = await _pageVersionsController.GetLatestVersion(pageId);

			// then
			actionResult.ShouldBeOkObjectResult();

			PageVersionResponse response = actionResult.GetOkObjectResultValue();
			response.ShouldNotBeNull();
			response.PageId.ShouldBe(pageId);
			response.Text.ShouldBe(pageVersion.Text);
		}

		[Fact]
		public async Task AllVersions()
		{
			// given
			List<PageVersion> pageVersions = _fixture.CreateMany<PageVersion>().ToList();

			_pageVersionRepositoryMock
				.AllVersionsAsync()
				.Returns(pageVersions);

			// when
			ActionResult<IEnumerable<PageVersionResponse>> actionResult = await _pageVersionsController.AllVersions();

			// then
			actionResult.ShouldBeOkObjectResult();

			IEnumerable<PageVersionResponse> responses = actionResult.GetOkObjectResultValue();
			responses.ShouldNotBeNull();
			responses.Count().ShouldBe(pageVersions.Count);
		}

		[Fact]
		public async Task FindPageVersionsByPageId()
		{
			// given
			List<PageVersion> pageVersions = _fixture.CreateMany<PageVersion>().ToList();
			int pageId = 42;
			pageVersions.ForEach(p => p.PageId = pageId);

			_pageVersionRepositoryMock
				.FindPageVersionsByPageIdAsync(pageId)
				.Returns(pageVersions);

			// when
			ActionResult<IEnumerable<PageVersionResponse>> actionResult = await _pageVersionsController.FindPageVersionsByPageId(pageId);

			// then
			actionResult.ShouldBeOkObjectResult();

			IEnumerable<PageVersionResponse> responses = actionResult.GetOkObjectResultValue();
			responses.ShouldNotBeNull();
			responses.Count().ShouldBe(pageVersions.Count);
		}

		[Fact]
		public async Task FindPageVersionsByAuthor()
		{
			// given
			List<PageVersion> pageVersions = _fixture.CreateMany<PageVersion>().ToList();
			string author = "weirdo";
			pageVersions.ForEach(p => p.Author = author);

			_pageVersionRepositoryMock
				.FindPageVersionsByAuthorAsync(author)
				.Returns(pageVersions);

			// when
			ActionResult<IEnumerable<PageVersionResponse>> actionResult = await _pageVersionsController.FindPageVersionsByAuthor(author);

			// then
			actionResult.ShouldBeOkObjectResult();

			IEnumerable<PageVersionResponse> responses = actionResult.GetOkObjectResultValue();
			responses.ShouldNotBeNull();
			responses.Count().ShouldBe(pageVersions.Count);
		}

		[Fact]
		public async Task Add()
		{
			// given
			int pageId = 1;
			string text = "text";
			string author = "author";
			DateTime dateTime = DateTime.UtcNow;

			var repoPageVersion = new PageVersion()
			{
				PageId = pageId,
				DateTime = dateTime,
				Author = author,
				Text = text
			};

			_pageVersionRepositoryMock
				.AddNewVersionAsync(pageId, text, author, dateTime)
				.Returns(repoPageVersion);

			// when
			ActionResult<PageVersionResponse> actionResult = await _pageVersionsController.Add(pageId, text, author, dateTime);

			// then
			actionResult.ShouldBeCreatedAtActionResult();

			PageVersionResponse response = actionResult.CreatedAtActionResultValue();
			response.PageId.ShouldBe(pageId);
			response.Text.ShouldBe(text);
			response.Author.ShouldBe(author);
			response.DateTime.ShouldBe(dateTime);
		}

		[Fact]
		public async Task Delete()
		{
			// given
			Guid pageVersionId = Guid.NewGuid();

			_pageVersionRepositoryMock
				.DeleteVersionAsync(pageVersionId)
				.Returns(Task.CompletedTask);

			// when
			ActionResult<string> actionResult = await _pageVersionsController.Delete(pageVersionId);

			// then
			actionResult.ShouldBeNoContentResult();
		}

		[Fact]
		public async Task Update()
		{
			// given
			var request = new PageVersionRequest()
			{
				Id = Guid.NewGuid(),
				Author = "buxton",
				DateTime = DateTime.Today,
				PageId = 42,
				Text = "Some new text"
			};

			var pageVersion = new PageVersion()
			{
				Id = request.Id,
				Author = request.Author,
				DateTime = request.DateTime,
				PageId = request.PageId,
				Text = request.Text,
			};

			_objectsConverterMock
				.ConvertToPageVersion(request)
				.Returns(pageVersion);

			_pageVersionRepositoryMock
				.UpdateExistingVersionAsync(pageVersion)
				.Returns(Task.CompletedTask);

			// when
			ActionResult<string> actionResult = await _pageVersionsController.Update(request);

			// then
			actionResult.ShouldBeNoContentResult();
		}
	}
}
