using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using Roadkill.Api.Common.Models;
using Roadkill.Api.Controllers;
using Roadkill.Api.ModelConverters;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public sealed class TagsControllerTests
	{
		private Mock<IPageRepository> _pageRepositoryMock;
		private TagsController _tagsController;
		private Fixture _fixture;
		private Mock<IPageModelConverter> _pageViewModelConverterMock;

		public TagsControllerTests()
		{
			_fixture = new Fixture();

			_pageViewModelConverterMock = new Mock<IPageModelConverter>();
			_pageViewModelConverterMock
				.Setup(x => x.ConvertToViewModel(It.IsAny<Page>()))
				.Returns<Page>(page => new PageModel() { Id = page.Id, Title = page.Title });

			_pageRepositoryMock = new Mock<IPageRepository>();
			_tagsController = new TagsController(_pageRepositoryMock.Object, _pageViewModelConverterMock.Object);
		}

		[Fact]
		public async Task AllTags_should_return_all_tags_fill_count_property()
		{
			// given
			List<string> tags = _fixture.CreateMany<string>().ToList();

			var duplicateTags = new List<string>();
			duplicateTags.Add("duplicate-tag");
			duplicateTags.Add("duplicate-tag");
			duplicateTags.Add("duplicate-tag");
			tags.AddRange(duplicateTags);

			int expectedTagCount = tags.Count - (duplicateTags.Count - 1);

			_pageRepositoryMock.Setup(x => x.AllTagsAsync())
				.ReturnsAsync(tags);

			// when
			IEnumerable<TagModel> tagViewModels = await _tagsController.AllTags();

			// then
			_pageRepositoryMock.Verify(x => x.AllTagsAsync(), Times.Once);
			tagViewModels.Count().ShouldBe(expectedTagCount);

			tagViewModels.First(x => x.Name == "duplicate-tag").Count.ShouldBe(3);
		}

		[Theory]
		[InlineData("tag1, typo-tag ", "tag1,fixed-tag")]
		[InlineData("tag1, typo-tag , tag3", "tag1,fixed-tag, tag3")]
		public async Task RenameTag_should_ignore_and_normalize_whitespace_for_tag(string existingTags, string expectedTags)
		{
			// given
			string tagToSearch = "typo-tag";
			string newTag = "fixed-tag";

			List<Page> pagesWithTags = _fixture.CreateMany<Page>().ToList();
			pagesWithTags.ForEach(p => { p.Tags = existingTags; });

			_pageRepositoryMock.Setup(x => x.FindPagesContainingTagAsync(tagToSearch))
				.ReturnsAsync(pagesWithTags);

			_pageRepositoryMock
				.Setup(x => x.UpdateExistingAsync(It.IsAny<Page>()))
				.ReturnsAsync(It.IsAny<Page>());

			// when
			await _tagsController.Rename(tagToSearch, newTag);

			// then
			_pageRepositoryMock.Verify(x => x.FindPagesContainingTagAsync(tagToSearch), Times.Once);
			_pageRepositoryMock.Verify(x => x.UpdateExistingAsync(It.Is<Page>(p => p.Tags == expectedTags)), Times.Exactly(pagesWithTags.Count));
		}

		[Fact]
		public async Task FindByTag_should_use_repository_to_find_tags()
		{
			// given
			List<Page> pages = _fixture.CreateMany<Page>().ToList();
			pages[0].Tags += ", gutentag";
			pages[1].Tags += ", gutentag";
			pages[2].Tags += ", gutentag";

			_pageRepositoryMock
				.Setup(x => x.FindPagesContainingTagAsync("gutentag"))
				.ReturnsAsync(pages);

			// when
			IEnumerable<PageModel> pageViewModelsWithTag = await _tagsController.FindPageWithTag("gutentag");

			// then
			pageViewModelsWithTag.Count().ShouldBe(pages.Count());

			_pageRepositoryMock.Verify(x => x.FindPagesContainingTagAsync("gutentag"), Times.Once);
			_pageViewModelConverterMock.Verify(x => x.ConvertToViewModel(It.IsAny<Page>()));
		}
	}
}
