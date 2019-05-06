using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.Controllers;
using Roadkill.Api.ModelConverters;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public sealed class PageVersionsControllerTests
	{
		private readonly Fixture _fixture;
		private readonly Mock<IPageVersionObjectsConverter> _objectsConverterMock;
		private readonly Mock<IPageVersionRepository> _pageVersionRepositoryMock;
		private PageVersionsController _pageVersionsController;

		public PageVersionsControllerTests()
		{
			_fixture = new Fixture();

			_pageVersionRepositoryMock = new Mock<IPageVersionRepository>();

			_objectsConverterMock = new Mock<IPageVersionObjectsConverter>();
			_objectsConverterMock
				.Setup(x => x.ConvertToPageVersionResponse(It.IsAny<PageVersion>()))
				.Returns<PageVersion>(pageVersion => new PageVersionResponse()
				{
					Id = pageVersion.Id,
					Text = pageVersion.Text,
					Author = pageVersion.Author,
					DateTime = pageVersion.DateTime,
					PageId = pageVersion.PageId
				});

			_pageVersionsController = new PageVersionsController(_pageVersionRepositoryMock.Object, _objectsConverterMock.Object);
		}

		[Fact]
		public async Task GetLatestVersion()
		{
			// given
			PageVersion pageVersion = _fixture.Create<PageVersion>();
			int pageId = pageVersion.PageId;

			_pageVersionRepositoryMock
				.Setup(x => x.GetLatestVersionAsync(pageId))
				.ReturnsAsync(pageVersion);

			// when
			PageVersionResponse response = await _pageVersionsController.GetLatestVersion(pageId);

			// then
			response.ShouldNotBeNull();
			response.PageId.ShouldBe(pageId);
			response.Text.ShouldBe(pageVersion.Text);

			_pageVersionRepositoryMock.Verify(x => x.GetLatestVersionAsync(pageId), Times.Once);
			_objectsConverterMock.Verify(x => x.ConvertToPageVersionResponse(pageVersion));
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
				.Setup(x => x.AddNewVersionAsync(pageId, text, author, dateTime))
				.ReturnsAsync(repoPageVersion);

			// when
			PageVersionResponse actualResponse = await _pageVersionsController.Add(pageId, text, author, dateTime);

			// then
			actualResponse.PageId.ShouldBe(pageId);
			actualResponse.Text.ShouldBe(text);
			actualResponse.Author.ShouldBe(author);
			actualResponse.DateTime.ShouldBe(dateTime);

			_pageVersionRepositoryMock
				.Verify(x => x.AddNewVersionAsync(pageId, text, author, dateTime), Times.Once);
		}

		[Fact]
		public async Task GetById()
		{
			// given
			PageVersion pageVersion = _fixture.Create<PageVersion>();
			Guid versionId = pageVersion.Id;

			_pageVersionRepositoryMock
				.Setup(x => x.GetByIdAsync(versionId))
				.ReturnsAsync(pageVersion);

			// when
			PageVersionResponse actualResponse = await _pageVersionsController.GetById(versionId);

			// then
			actualResponse.ShouldNotBeNull();
			actualResponse.Id.ShouldBe(versionId);

			_pageVersionRepositoryMock.Verify(x => x.GetByIdAsync(versionId), Times.Once);
			_objectsConverterMock.Verify(x => x.ConvertToPageVersionResponse(pageVersion), Times.Once);
		}

		[Fact]
		public async Task Delete()
		{
			// given
			Guid pageVersionId = Guid.NewGuid();

			_pageVersionRepositoryMock
				.Setup(x => x.DeleteVersionAsync(pageVersionId))
				.Returns(Task.CompletedTask);

			// when
			await _pageVersionsController.Delete(pageVersionId);

			// then
			_pageVersionRepositoryMock
				.Verify(x => x.DeleteVersionAsync(pageVersionId), Times.Once);
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

			_objectsConverterMock.Setup(x => x.ConvertToPageVersion(request))
				.Returns(pageVersion);

			_pageVersionRepositoryMock
				.Setup(x => x.UpdateExistingVersionAsync(pageVersion))
				.Returns(Task.CompletedTask);

			// when
			await _pageVersionsController.Update(request);

			// then
			_pageVersionRepositoryMock.Verify(x => x.UpdateExistingVersionAsync(pageVersion), Times.Once);
			_objectsConverterMock.Verify(x => x.ConvertToPageVersion(request));
		}

		[Fact]
		public async Task AllVersions()
		{
			// given
			List<PageVersion> pageVersions = _fixture.CreateMany<PageVersion>().ToList();

			_pageVersionRepositoryMock
				.Setup(x => x.AllVersionsAsync())
				.ReturnsAsync(pageVersions);

			// when
			IEnumerable<PageVersionResponse> actualResponses = await _pageVersionsController.AllVersions();

			// then
			actualResponses.ShouldNotBeNull();
			actualResponses.Count().ShouldBe(pageVersions.Count);

			_pageVersionRepositoryMock.Verify(x => x.AllVersionsAsync(), Times.Once);
			_objectsConverterMock.Verify(x => x.ConvertToPageVersionResponse(It.IsAny<PageVersion>()), Times.Exactly(pageVersions.Count));
		}

		[Fact]
		public async Task FindPageVersionsByPageId()
		{
			// given
			List<PageVersion> pageVersions = _fixture.CreateMany<PageVersion>().ToList();
			int pageId = 42;
			pageVersions.ForEach(p => p.PageId = pageId);

			_pageVersionRepositoryMock
				.Setup(x => x.FindPageVersionsByPageIdAsync(pageId))
				.ReturnsAsync(pageVersions);

			// when
			IEnumerable<PageVersionResponse> actualResponses = await _pageVersionsController.FindPageVersionsByPageId(pageId);

			// then
			actualResponses.ShouldNotBeNull();
			actualResponses.Count().ShouldBe(pageVersions.Count);

			_pageVersionRepositoryMock.Verify(x => x.FindPageVersionsByPageIdAsync(pageId), Times.Once);
			_objectsConverterMock.Verify(x => x.ConvertToPageVersionResponse(It.IsAny<PageVersion>()), Times.Exactly(pageVersions.Count));
		}

		[Fact]
		public async Task FindPageVersionsByAuthor()
		{
			// given
			List<PageVersion> pageVersions = _fixture.CreateMany<PageVersion>().ToList();
			string author = "weirdo";
			pageVersions.ForEach(p => p.Author = author);

			_pageVersionRepositoryMock
				.Setup(x => x.FindPageVersionsByAuthorAsync(author))
				.ReturnsAsync(pageVersions);

			// when
			IEnumerable<PageVersionResponse> actualResponses = await _pageVersionsController.FindPageVersionsByAuthor(author);

			// then
			actualResponses.ShouldNotBeNull();
			actualResponses.Count().ShouldBe(pageVersions.Count);

			_pageVersionRepositoryMock.Verify(x => x.FindPageVersionsByAuthorAsync(author), Times.Once);
			_objectsConverterMock.Verify(x => x.ConvertToPageVersionResponse(It.IsAny<PageVersion>()), Times.Exactly(pageVersions.Count));
		}
	}
}
