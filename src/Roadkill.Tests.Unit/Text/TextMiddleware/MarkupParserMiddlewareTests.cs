using NSubstitute;
using Roadkill.Text.Models;
using Roadkill.Text.Parsers;
using Roadkill.Text.TextMiddleware;
using Xunit;

namespace Roadkill.Tests.Unit.Text.TextMiddleware
{
	public class MarkupParserMiddlewareTests
	{
		[Fact]
		public void should_parser_markup_using_parser()
		{
			// Arrange
			string markdown = "some **bold** text";
			string expectedHtml = "<p>some <strong>bold</strong> text</p>\n";

			var pagehtml = new PageHtml() { Html = markdown };

			var parser = Substitute.For<IMarkupParser>();
			parser
				.ToHtml(markdown)
				.Returns(expectedHtml);

			var middleware = new MarkupParserMiddleware(parser);

			// Act
			PageHtml actualPageHtml = middleware.Invoke(pagehtml);

			// Assert
			Assert.Equal(expectedHtml, actualPageHtml.Html);
		}
	}
}
