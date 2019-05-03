using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Roadkill.Text;
using Roadkill.Text.Parsers.Links;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Text.Parsers.Links
{
	public class LinkHrefParserTests
	{
		// Many of these tests were converted from the v1.7 MarkdownConverter tests.
		private TextSettings _textSettings;

		private LinkHrefParser _linkHrefParser;
		private Mock<IUrlHelper> _urlHelperMock;
		private Mock<IPageRepository> _pageRepository;

		public LinkHrefParserTests()
		{
			_pageRepository = new Mock<IPageRepository>();
			_textSettings = new TextSettings();
			_urlHelperMock = new Mock<IUrlHelper>();
			_urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(s => s);

			_linkHrefParser = new LinkHrefParser(_pageRepository.Object, _textSettings, _urlHelperMock.Object);
		}

		[Fact]
		public void href_with_dashes_and_23_are_not_encoded()
		{
			// Arrange
			HtmlLinkTag linkTag = new HtmlLinkTag("https://www.google.com/some-page-23", "https://www.google.com/some-page-23", "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("https://www.google.com/some-page-23");
		}

		[Fact]
		public void href_links_with_the_word_script_in_url_should_not_be_cleaned()
		{
			// Arrange - Issue #159 (Bitbucket) (deSCRIPTion)
			HtmlLinkTag linkTag = new HtmlLinkTag("http://msdn.microsoft.com/en-us/library/system.componentmodel.descriptionattribute.aspx", "http://msdn.microsoft.com/en-us/library/system.componentmodel.descriptionattribute.aspx", "Component description", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("http://msdn.microsoft.com/en-us/library/system.componentmodel.descriptionattribute.aspx");
		}

		[Theory]
		[InlineData("http://www.example.com")]
		[InlineData("https://www.example.com")]
		[InlineData("www.example.com")]
		[InlineData("mailto:me@example.com")]
		[InlineData("tag:the-architecture-of-old")]
		[SuppressMessage("Stylecop", "CA1054", Justification = "It's ok to not use a URI, I said so.")]
		public void should_add_external_links_css_class_to_links_and_keep_url(string url)
		{
			// Arrange
			HtmlLinkTag linkTag = new HtmlLinkTag(url, url, "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe(url);
			actualTag.CssClass.ShouldBe("external-link");
		}

		[Fact]
		public void should_not_add_external_link_cssclass_for_anchor_tags()
		{
			// Arrange
			HtmlLinkTag linkTag = new HtmlLinkTag("#my-anchor", "#my-anchor", "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("#my-anchor");
			actualTag.CssClass.ShouldBe("");
		}

		[Theory]
		[InlineData("attachment:/")]
		[InlineData("~/")]
		public void href_links_starting_with_attachments_should_resolve_as_attachment_paths(string prefix)
		{
			// Arrange
			string actualPath = $"{prefix}my/folder/image1.jpg";
			HtmlLinkTag linkTag = new HtmlLinkTag(actualPath, actualPath, "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("/attachments/my/folder/image1.jpg");
		}

		[Fact]
		public void should_use_url_helper_for_special_pages()
		{
			// Arrange
			HtmlLinkTag linkTag = new HtmlLinkTag("Special:blah", "Special:blah", "text", "");
			_urlHelperMock.Setup(x => x.Content(It.IsAny<string>()))
				.Returns("~/wiki/Special:blah/url-helper");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("~/wiki/Special:blah/url-helper");
		}

		[Fact]
		public void links_starting_with_special_should_resolve_as_full_specialpage()
		{
			// Arrange
			HtmlLinkTag linkTag = new HtmlLinkTag("Special:Foo", "Special:Foo", "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("~/wiki/Special:Foo");
		}

		[Fact]
		public void href_external_link_with_anchor_should_retain_anchor()
		{
			// Arrange - Issue #172 (Bitbucket)
			HtmlLinkTag linkTag = new HtmlLinkTag("http://www.google.com/?blah=xyz#myanchor", "http://www.google.com/?blah=xyz#myanchor", "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("http://www.google.com/?blah=xyz#myanchor");
		}

		[Fact]
		public void href_internal_links_with_querystring_and_anchor_tag_should_find_page_and_retain_querystring_and_anchor()
		{
			// Arrange
			string pageTitle = "foo page";
			Page dummyPage = new Page() { Id = 1, Title = pageTitle };
			dynamic urlValues = new { id = dummyPage.Id, title = "foo-page" };

			_urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
						  .Returns<UrlActionContext>(s => "/wiki/" + urlValues.id + "/" + urlValues.title);

			_pageRepository.Setup(x => x.GetPageByTitleAsync(pageTitle)).ReturnsAsync(dummyPage);

			HtmlLinkTag linkTag = new HtmlLinkTag("foo-page?blah=xyz#myanchor", "foo-page?blah=xyz#myanchor", "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("/wiki/1/foo-page?blah=xyz#myanchor");
		}

		[Fact]
		public void href_external_link_with_urlencoded_anchor_should_retain_anchor()
		{
			// Arrange - Issue #172 (Bitbucket)
			HtmlLinkTag linkTag = new HtmlLinkTag("http://www.google.com/?blah=xyz%23myanchor", "http://www.google.com/?blah=xyz%23myanchor", "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("http://www.google.com/?blah=xyz%23myanchor");
		}

		[Fact]
		public void href_internal_links_with_anchor_tag_should_retain_anchor()
		{
			// Arrange
			string expectedHref = "/wiki/1/foo";
			_urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(expectedHref);

			string pageTitle = "foo";
			Page dummyPage = new Page() { Id = 1, Title = pageTitle };
			_pageRepository.Setup(x => x.GetPageByTitleAsync(pageTitle)).ReturnsAsync(dummyPage);

			HtmlLinkTag linkTag = new HtmlLinkTag("foo#myanchor", "foo#myanchor", "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("/wiki/1/foo#myanchor");
		}

		[Fact]
		public void should_remove_dashes_in_title_and_find_page_in_repository()
		{
			// Arrange
			string expectedHref = "/wiki/1/my-page-on-engineering";
			_urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(expectedHref);

			string pageTitle = "foo";
			Page dummyPage = new Page() { Id = 1, Title = pageTitle };
			_pageRepository.Setup(x => x.GetPageByTitleAsync(pageTitle)).ReturnsAsync(dummyPage);
			HtmlLinkTag linkTag = new HtmlLinkTag("my-page-on-engineering", "my-page-on-engineering", "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.OriginalHref.ShouldBe("my-page-on-engineering");
			actualTag.Href.ShouldBe("/wiki/1/my-page-on-engineering");
		}

		[Fact]
		public void href_internal_existing_wiki_page_link_should_return_href_with_wiki_prefix()
		{
			// Arrange
			string expectedHref = "/wiki/1/football";
			_urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(expectedHref);

			string pageTitle = "foo";
			Page dummyPage = new Page() { Id = 1, Title = pageTitle };
			_pageRepository.Setup(x => x.GetPageByTitleAsync(pageTitle)).ReturnsAsync(dummyPage);

			HtmlLinkTag linkTag = new HtmlLinkTag("football", "foo-page", "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("/wiki/1/football");
		}

		[Fact]
		public void should_add_missing_page_link_css_class_when_internal_link_does_not_exist()
		{
			// Arrange
			HtmlLinkTag linkTag = new HtmlLinkTag("doesnt-exist", "doesnt-exist", "text", "");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.CssClass.ShouldBe("missing-page-link");
		}

		[Fact]
		public void should_set_href_and_target_proprties()
		{
			// Arrange
			string expectedHref = "/wiki/1/despair";
			_urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(expectedHref);

			string pageTitle = "foo";
			Page dummyPage = new Page() { Id = 1, Title = pageTitle };
			_pageRepository.Setup(x => x.GetPageByTitleAsync(pageTitle)).ReturnsAsync(dummyPage);

			HtmlLinkTag linkTag = new HtmlLinkTag("despair", "", "text", "new");

			// Act
			HtmlLinkTag actualTag = _linkHrefParser.Parse(linkTag);

			// Assert
			actualTag.Href.ShouldBe("/wiki/1/despair");
			actualTag.Target.ShouldBe("");
		}
	}
}
