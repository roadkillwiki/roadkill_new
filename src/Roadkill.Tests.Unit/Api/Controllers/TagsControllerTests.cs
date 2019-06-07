using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
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
		public void Controller_should_require_admin_access()
		{
			Type attributeType = typeof(AuthorizeAttribute);

			var customAttributes = typeof(TagsController).GetCustomAttributes(attributeType, false);
			customAttributes.Length.ShouldBeGreaterThan(0, $"No {attributeType.Name} found for TagsController");

			AuthorizeAttribute authorizeAttribute = customAttributes[0] as AuthorizeAttribute;
			authorizeAttribute?.Policy.ShouldNotBeNullOrEmpty("No AuthorizeAttribute policy string specified for TagsController");
			authorizeAttribute?.Policy.ShouldContain(PolicyNames.Admin);
		}

		[Theory]
		[InlineData(nameof(TagsController.AllTags))]
		[InlineData(nameof(TagsController.FindPageWithTag))]
		public void Get_methods_should_be_HttpGet_with_custom_routeTemplate_and_allow_anonymous(string methodName, string routeTemplate = "")
		{
			Type attributeType = typeof(HttpGetAttribute);
			if (string.IsNullOrEmpty(routeTemplate))
			{
				routeTemplate = methodName;
			}

			_tagsController.ShouldHaveAttribute(methodName, attributeType);
			_tagsController.ShouldHaveRouteAttributeWithTemplate(methodName, routeTemplate);
		}

		[Fact]
		public void Rename_should_be_HttpPut_and_allow_editors()
		{
			string methodName = nameof(TagsController.Rename);
			Type attributeType = typeof(HttpPutAttribute);

			_tagsController.ShouldHaveAttribute(methodName, attributeType);
		}

		[Fact]
		public async Task AllTags_should_return_all_tags_and_fill_count_property()
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
			ActionResult<IEnumerable<TagResponse>> actionResult = await _tagsController.AllTags();

			// then
			actionResult.ShouldBeOkObjectResult();
			IEnumerable<TagResponse> response = actionResult.GetOkObjectResultValue();

			response.Count().ShouldBe(expectedTagCount);
			response.First(x => x.Name == "duplicate-tag").Count.ShouldBe(3);
		}

		[Theory]
		[InlineData("tag1, typo-tag ", "tag1,fixed-tag")]
		[InlineData("tag1, typo-tag , tag3", "tag1,fixed-tag, tag3")]
		public async Task RenameTag_should_ignore_and_normalize_whitespace_for_tag_and_return_nocontent(string existingTags, string expectedTags)
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
			ActionResult<string> actionResult = await _tagsController.Rename(tagToSearch, newTag);

			// then
			actionResult.ShouldBeNoContentResult();

			foreach (Page page in pagesWithTags)
			{
				page.Tags.ShouldBe(expectedTags);
			}
		}

		[Fact]
		public async Task FindPageWithTag_should_return_pages()
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
			ActionResult<IEnumerable<PageResponse>> actionResult = await _tagsController.FindPageWithTag(tag);

			// then
			actionResult.ShouldBeOkObjectResult();
			IEnumerable<PageResponse> response = actionResult.GetOkObjectResultValue();

			response.Count()
					.ShouldBe(pagesWithTag.Count());
		}
	}
}
