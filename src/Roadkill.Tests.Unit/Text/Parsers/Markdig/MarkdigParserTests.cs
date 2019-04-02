using System;
using Roadkill.Text.Parsers.Images;
using Roadkill.Text.Parsers.Links;
using Roadkill.Text.Parsers.Markdig;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Text.Parsers.Markdig
{
	public class MarkdigParserTests
	{
		[Fact]
		public void should_ignore_null_imageparsed_and_linkparsed_funcs()
		{
			// given
			string expectedHtml = "<p><a href=\"http://www.google.com\" rel=\"nofollow\">googz</a>" +
									"<img src=\"/myimage.jpg\" class=\"img-responsive\" border=\"0\" alt=\"img\" title=\"img\" /></p>\n";
			var parser = new MarkdigParser();

			// when
			string actualHtml = parser.ToHtml("[googz](http://www.google.com)![img](/myimage.jpg)");

			// then
			expectedHtml.ShouldBe(actualHtml);
		}

		[Fact]
		public void should_bind_imageparsed_and_linkparsed_funcs()
		{
			// given
			Func<HtmlImageTag, HtmlImageTag> imageParsed = (imageTag) =>
			{
				imageTag.Title = "new title";
				imageTag.Alt = "new alt";
				imageTag.Src = "/new src";
				return imageTag;
			};

			Func<HtmlLinkTag, HtmlLinkTag> linkParsed = (linkTag) =>
			{
				linkTag.CssClass = "new css";
				linkTag.Href = "/new href";
				linkTag.Text = "new text";

				return linkTag;
			};

			string expectedHtml = "<p><a href=\"/new%20href\" class=\"new css\">new text</a>" +
									"<img src=\"/new%20src\" class=\"img-responsive\" border=\"0\" alt=\"new alt\" title=\"new title\" /></p>\n";
			var parser = new MarkdigParser();
			parser.ImageParsed = imageParsed;
			parser.LinkParsed = linkParsed;

			// when
			string actualHtml = parser.ToHtml("[googz](http://www.google.com)![img](/myimage.jpg)");

			// then
			expectedHtml.ShouldBe(actualHtml);
		}

		[Fact]
		public void should_retain_anchor_tags_and_querystrings()
		{
			// Arrange
			string expectedHtml = "<p><a href=\"?a=%23myvalue#myanchortag\">hello world</a></p>\n";
			string markdown = "[hello world](?a=%23myvalue#myanchortag)";
			var parser = new MarkdigParser();

			// when
			string html = parser.ToHtml(markdown);

			// then
			expectedHtml.ShouldBe(html);
		}

		[Fact]
		public void should_handle_empty_strings()
		{
			// given
			var parser = new MarkdigParser();

			// when
			string html = parser.ToHtml(null);

			// then
			html.ShouldBeEmpty();
		}

		[Fact]
		public void should_parse_basic_markdown()
		{
			// given
			string expectedHtml = "<p><a href=\"http://www.google.com\" class=\"main\" rel=\"nofollow\">i am a link</a></p>\n";
			string markdown = "[i am a link](http://www.google.com){.main}";
			var parser = new MarkdigParser();

			// when
			string html = parser.ToHtml(markdown);

			// then
			expectedHtml.ShouldBe(html);
		}

		[Fact]
		public void should_parse_advanced_markdown_with_tasks_autoids_css_and_autolinking()
		{
			// given - strikethroughs aren't working yet with Markdig
			string markdown = @"- [ ] Task 1
- [ ] Task 2

# myh1 with a class { .main }

http://www.google.com
";
			string expectedHtml = @"<ul class=""contains-task-list"">
<li class=""task-list-item""> Task 1</li>
<li class=""task-list-item""> Task 2</li>
</ul>
<h1 id=""myh1-with-a-class"" class=""main"">myh1 with a class</h1>
<p><a href=""http://www.google.com"" rel=""nofollow"">http://www.google.com</a></p>" + "\n";

			var parser = new MarkdigParser();

			// when
			string actualHtml = parser.ToHtml(markdown);

			// then
			expectedHtml = expectedHtml.Replace("\n", "", StringComparison.Ordinal)
									   .Replace("\r", "", StringComparison.Ordinal);
			actualHtml = actualHtml.Replace("\n", "", StringComparison.Ordinal)
								   .Replace("\r", "", StringComparison.Ordinal);
			expectedHtml.ShouldBe(actualHtml);
		}
	}
}
