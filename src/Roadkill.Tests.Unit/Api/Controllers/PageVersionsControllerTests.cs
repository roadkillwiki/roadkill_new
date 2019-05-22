using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.Controllers;
using Roadkill.Api.JWT;
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
		public void Add_should_be_HttpPost_and_allow_editors()
		{
			string methodName = nameof(PageVersionsController.Add);
			Type attributeType = typeof(HttpPostAttribute);

			_pageVersionsController.ShouldHaveAttribute(methodName, attributeType);
			_pageVersionsController.ShouldAuthorizeEditors(methodName);
		}

		[Fact]
		public void Update_should_be_HttpPut_and_allow_editors()
		{
			string methodName = nameof(PageVersionsController.Update);
			Type attributeType = typeof(HttpPutAttribute);

			_pageVersionsController.ShouldHaveAttribute(methodName, attributeType);
			_pageVersionsController.ShouldAuthorizeAdmins(methodName);
		}

		[Fact]
		public void Delete_should_be_HttpDelete_and_allow_admins()
		{
			string methodName = nameof(PageVersionsController.Delete);
			Type attributeType = typeof(HttpDeleteAttribute);

			_pageVersionsController.ShouldHaveAttribute(methodName, attributeType);
			_pageVersionsController.ShouldAuthorizeAdmins(methodName);
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
			PageVersionResponse actualResponse = await _pageVersionsController.Get(versionId);

			// then
			actualResponse.ShouldNotBeNull();
			actualResponse.Id.ShouldBe(versionId);

			await _pageVersionRepositoryMock
				.Received(1)
				.GetByIdAsync(versionId);

			_objectsConverterMock
				.Received(1)
				.ConvertToPageVersionResponse(pageVersion);
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
			PageVersionResponse response = await _pageVersionsController.GetLatestVersion(pageId);

			// then
			response.ShouldNotBeNull();
			response.PageId.ShouldBe(pageId);
			response.Text.ShouldBe(pageVersion.Text);

			_pageVersionRepositoryMock
				.Received(1)
				.GetLatestVersionAsync(pageId);

			_objectsConverterMock
				.Received(1)
				.ConvertToPageVersionResponse(pageVersion);
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
			IEnumerable<PageVersionResponse> actualResponses = await _pageVersionsController.AllVersions();

			// then
			actualResponses.ShouldNotBeNull();
			actualResponses.Count().ShouldBe(pageVersions.Count);

			await _pageVersionRepositoryMock
				.Received(1)
				.AllVersionsAsync();

			_objectsConverterMock
				.Received(pageVersions.Count)
				.ConvertToPageVersionResponse(Arg.Any<PageVersion>());
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
			IEnumerable<PageVersionResponse> actualResponses = await _pageVersionsController.FindPageVersionsByPageId(pageId);

			// then
			actualResponses.ShouldNotBeNull();
			actualResponses.Count().ShouldBe(pageVersions.Count);

			await _pageVersionRepositoryMock
				.Received(1)
				.FindPageVersionsByPageIdAsync(pageId);

			_objectsConverterMock
				.Received(pageVersions.Count)
				.ConvertToPageVersionResponse(Arg.Any<PageVersion>());
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
			IEnumerable<PageVersionResponse> actualResponses = await _pageVersionsController.FindPageVersionsByAuthor(author);

			// then
			actualResponses.ShouldNotBeNull();
			actualResponses.Count().ShouldBe(pageVersions.Count);

			await _pageVersionRepositoryMock
				.Received(1)
				.FindPageVersionsByAuthorAsync(author);

			_objectsConverterMock
				.Received(pageVersions.Count)
				.ConvertToPageVersionResponse(Arg.Any<PageVersion>());
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
			PageVersionResponse actualResponse = await _pageVersionsController.Add(pageId, text, author, dateTime);

			// then
			actualResponse.PageId.ShouldBe(pageId);
			actualResponse.Text.ShouldBe(text);
			actualResponse.Author.ShouldBe(author);
			actualResponse.DateTime.ShouldBe(dateTime);

			await _pageVersionRepositoryMock
				.Received(1)
				.AddNewVersionAsync(pageId, text, author, dateTime);
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
			await _pageVersionsController.Delete(pageVersionId);

			// then
			await _pageVersionRepositoryMock
				.Received(1)
				.DeleteVersionAsync(pageVersionId);
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
			await _pageVersionsController.Update(request);

			// then
			await _pageVersionRepositoryMock
				.Received(1)
				.UpdateExistingVersionAsync(pageVersion);

			_objectsConverterMock
				.Received(1)
				.ConvertToPageVersion(request);
		}
	}
}
