using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Roadkill.Text;
using Roadkill.Text.CustomTokens;
using Roadkill.Text.Models;
using Roadkill.Text.TextMiddleware;
using Xunit;

namespace Roadkill.Tests.Unit.Text.TextMiddleware
{
	public class CustomTokenMiddlewareTests
	{
		private ILogger _logger;

		public CustomTokenMiddlewareTests()
		{
			_logger = Mock.Of<ILogger>();
		}

		[Fact]
		public void should_clean_html_using_sanitizer()
		{
			string markdown = @"@@warningbox:ENTER YOUR CONTENT HERE

here is some more content

@@";

			string expectedHtml = @"<div class=""alert alert-warning"">ENTER YOUR CONTENT HERE

here is some more content

</div><br style=""clear:both""/>";

			var pagehtml = new PageHtml() { Html = markdown };

			var settings = new TextSettings();
			settings.CustomTokensPath = Path.Combine(Directory.GetCurrentDirectory(), "Text", "CustomTokens", "customvariables.xml");

			var customTokenParser = new CustomTokenParser(settings, _logger);
			var middleware = new CustomTokenMiddleware(customTokenParser);

			// Act
			PageHtml actualPageHtml = middleware.Invoke(pagehtml);

			actualPageHtml.Html = actualPageHtml.Html.Replace(Environment.NewLine, "", StringComparison.Ordinal);
			expectedHtml = expectedHtml.Replace(Environment.NewLine, "", StringComparison.Ordinal);

			// Assert
			Assert.Equal(expectedHtml, actualPageHtml.Html);
		}
	}
}
