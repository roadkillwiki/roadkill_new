using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using Roadkill.Api.Common.Response;
using Roadkill.Api.Controllers;
using Roadkill.Api.ObjectConverters;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public sealed class TagsControllerTests
	{
		private IPageRepository _pageRepositoryMock;
		private TagsController _tagsController;
		private Fixture _fixture;
		private IPageObjectsConverter _pageViewModelConverterMock;

		public TagsControllerTests()
		{
			_fixture = new Fixture();

			_pageViewModelConverterMock = Substitute.For<IPageObjectsConverter>();
			_pageViewModelConverterMock
				.ConvertToPageResponse(Arg.Any<Page>())
				.Returns(callInfo =>
				{
					var page = callInfo.Arg<Page>();
					return new PageResponse()
					{
						Id = page.Id,
						Title = page.Title
					};
				});

			_pageRepositoryMock = Substitute.For<IPageRepository>();
			_tagsController = new TagsController(_pageRepositoryMock, _pageViewModelConverterMock);
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

			_pageRepositoryMock
				.AllTagsAsync()
				.Returns(tags);

			// when
			IEnumerable<TagResponse> tagViewModels = await _tagsController.AllTags();

			// then
			await _pageRepositoryMock
				.Received(1)
				.AllTagsAsync();

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

			_pageRepositoryMock
				.FindPagesContainingTagAsync(tagToSearch)
				.Returns(pagesWithTags);

			// when
			await _tagsController.Rename(tagToSearch, newTag);

			// then
			await _pageRepositoryMock
				.Received(1)
				.FindPagesContainingTagAsync(tagToSearch);

			await _pageRepositoryMock
				.Received(pagesWithTags.Count)
				.UpdateExistingAsync(Arg.Is<Page>(p => p.Tags == expectedTags));
		}

		[Fact]
		public async Task FindByTag_should_use_repository_to_find_tags()
		{
			// given
			string tag = "gutentag";

			List<Page> pagesWithTag = _fixture.CreateMany<Page>().ToList();
			pagesWithTag[0].Tags += $", {tag}";
			pagesWithTag[1].Tags += $", {tag}";
			pagesWithTag[2].Tags += $", {tag}";

			_pageRepositoryMock
				.FindPagesContainingTagAsync(tag)
				.Returns(pagesWithTag);

			// when
			IEnumerable<PageResponse> pageViewModelsWithTag = await _tagsController.FindPageWithTag(tag);

			// then
			pageViewModelsWithTag.Count().ShouldBe(pagesWithTag.Count());

			_pageViewModelConverterMock
				.Received(pagesWithTag.Count)
				.ConvertToPageResponse(Arg.Any<Page>());
		}
	}
}
