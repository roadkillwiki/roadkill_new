using Ganss.XSS;
using NSubstitute;
using Roadkill.Text.Models;
using Roadkill.Text.Sanitizer;
using Roadkill.Text.TextMiddleware;
using Xunit;

namespace Roadkill.Tests.Unit.Text.TextMiddleware
{
	public class HarmfulTagMiddlewareTests
	{
		[Fact]
		public void should_handle_null_sanitizer_from_factory_and_return_uncleaned_html()
		{
			// Arrange
			string html = "<div onclick=\"javascript:alert('ouch');\">test</div>";
			var pagehtml = new PageHtml() { Html = html };

			var factoryMock = Substitute.For<IHtmlSanitizerFactory>();
			factoryMock
				.CreateHtmlSanitizer()
				.Returns(callInfo => null);

			var middleware = new HarmfulTagMiddleware(factoryMock);

			// Act
			PageHtml actualPageHtml = middleware.Invoke(pagehtml);

			// Assert
			Assert.Equal(html, actualPageHtml.Html);
		}

		[Fact]
		public void should_clean_html_using_sanitizer()
		{
			// Arrange
			string html = "<div onclick=\"javascript:alert('ouch');\">test</div>";
			var pagehtml = new PageHtml() { Html = html };

			var htmlSanitizerMock = Substitute.For<IHtmlSanitizer>();

			var factoryMock = Substitute.For<IHtmlSanitizerFactory>();
			factoryMock
				.CreateHtmlSanitizer()
				.Returns(htmlSanitizerMock);

			var middleware = new HarmfulTagMiddleware(factoryMock);

			// Act
			middleware.Invoke(pagehtml);

			// Assert
			htmlSanitizerMock
				.Received(1)
				.Sanitize(html, "", null);
		}
	}
}
