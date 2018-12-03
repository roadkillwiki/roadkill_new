using System.IO;
using System.Text;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Roadkill.Text.Parsers.Markdig;
using Xunit;

namespace Roadkill.Tests.Unit.Text.Parsers.Markdig
{
    public class MarkdigImageAndLinkWalkerTests
    {
        private MarkdownDocument CreateMarkdownObject(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            MarkdownDocument doc = Markdown.Parse(markdown, pipeline);
            return doc;
        }

        private string ConvertToHtml(MarkdownObject markdownObject)
        {
            var builder = new StringBuilder();
            var textwriter = new StringWriter(builder);

            var renderer = new HtmlRenderer(textwriter);
            renderer.Render(markdownObject);

            return builder.ToString();
        }

        [Fact]
        public void should_call_image_and_link_events()
        {
            // given
            bool imageParsed = false;
            bool linkParsed = false;

            MarkdownDocument markdownObject = CreateMarkdownObject("![img](img.jpg) and a link [link text](http://www.foo.com)");
            var walker = new MarkdigImageAndLinkWalker(image => { imageParsed = true; }, link => { linkParsed = true; });

            // when
            walker.WalkAndBindParseEvents(markdownObject);

            // walk
            Assert.True(imageParsed);
            Assert.True(linkParsed);
        }

        [Fact]
        public void should_ignore_null_image_handler()
        {
            // given
            MarkdownDocument markdownObject = CreateMarkdownObject("![img](serverless.jpg)");
            var walker = new MarkdigImageAndLinkWalker(null, link => { });

            // when + then
            walker.WalkAndBindParseEvents(markdownObject);
        }

        [Fact]
        public void should_ignore_null_link_handler()
        {
            // given
            MarkdownDocument markdownObject = CreateMarkdownObject("[text](serverless.hyml)");
            var walker = new MarkdigImageAndLinkWalker(image => { }, null);

            // when + then
            walker.WalkAndBindParseEvents(markdownObject);
        }

        [Fact]
        public void should_add_css_and_attributes_to_links_from_delegate()
        {
            // given
            MarkdownDocument markdownObject = CreateMarkdownObject("[text](serverless.html)");
            var walker = new MarkdigImageAndLinkWalker(null, link =>
            {
                link.CssClass = "my-class";
                link.Href = "new-href";
                link.Target = "new-target";
                link.Text = "new-text";
            });

            // when
            walker.WalkAndBindParseEvents(markdownObject);
            string html = ConvertToHtml(markdownObject);

            // then
            Assert.Equal("<p><a href=\"new-href\" class=\"my-class\" target=\"new-target\">new-text</a></p>\n", html);
        }

        [Fact]
        public void should_add_no_follow_to_external_protocols()
        {
            // given
            string markdown = "[my email](mailto:email@example.com)\n";
            markdown += "[my http site](http://www.googlez.com)\n";
            markdown += "[my https site](https://www.littlebamboo.com)\n";

            MarkdownDocument markdownObject = CreateMarkdownObject(markdown);
            var walker = new MarkdigImageAndLinkWalker(null, null);

            // when
            walker.WalkAndBindParseEvents(markdownObject);
            string html = ConvertToHtml(markdownObject);

            // then
            Assert.Contains("<a href=\"mailto:email@example.com\" rel=\"nofollow\">my email</a>", html);
            Assert.Contains("<a href=\"http://www.googlez.com\" rel=\"nofollow\">my http site</a>", html);
            Assert.Contains("<a href=\"https://www.littlebamboo.com\" rel=\"nofollow\">my https site</a>", html);
        }

        [Fact]
        public void should_add_alt_attribute_and_bootstrap_class_and_border_attribute()
        {
            // given
            MarkdownDocument markdownObject = CreateMarkdownObject("![my image](meme.jpg)");
            var walker = new MarkdigImageAndLinkWalker(null, null);

            // when
            walker.WalkAndBindParseEvents(markdownObject);
            string html = ConvertToHtml(markdownObject);

            // then
            Assert.Equal("<p><img src=\"meme.jpg\" class=\"img-responsive\" border=\"0\" alt=\"my image\" title=\"my image\" /></p>\n", html);
        }

        [Fact]
        public void should_change_url_and_title_from_handler()
        {
            // given
            MarkdownDocument markdownObject = CreateMarkdownObject("![my image](meme.jpg)");
            var walker = new MarkdigImageAndLinkWalker(image =>
            {
                image.Alt = "new-alt";
                image.Title = "new-title";
                image.Src = "new-image.jpg";
            }, null);

            // when
            walker.WalkAndBindParseEvents(markdownObject);
            string html = ConvertToHtml(markdownObject);

            // then
            Assert.Equal("<p><img src=\"new-image.jpg\" class=\"img-responsive\" border=\"0\" alt=\"new-alt\" title=\"new-title\" /></p>\n", html);
        }

        [Fact]
        public void should_return_empty_when_markdown_is_empty()
        {
            // given
            MarkdownDocument markdownObject = CreateMarkdownObject("");
            var walker = new MarkdigImageAndLinkWalker(null, null);

            // when
            walker.WalkAndBindParseEvents(markdownObject);
            string html = ConvertToHtml(markdownObject);

            // then
            Assert.Equal("", html);
        }
    }
}