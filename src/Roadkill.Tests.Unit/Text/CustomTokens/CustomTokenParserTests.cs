using System.IO;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Roadkill.Text;
using Roadkill.Text.CustomTokens;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Text.CustomTokens
{
	public class CustomTokenParserTests
	{
		private ILogger _logger;

		static CustomTokenParserTests()
		{
			CustomTokenParser.CacheTokensFile = false;
		}

		public CustomTokenParserTests()
		{
			_logger = Substitute.For<ILogger>();
		}

		[Fact]
		public void should_contain_empty_list_when_tokens_file_not_found()
		{
			// Arrange
			TextSettings settings = new TextSettings();
			settings.CustomTokensPath = Path.Combine(Directory.GetCurrentDirectory(), "Unit", "Text", "CustomTokens", "doesntexist.xml");
			CustomTokenParser parser = new CustomTokenParser(settings, _logger);

			string expectedHtml = "@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@";

			// Act
			string actualHtml = parser.ReplaceTokensAfterParse("@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@");

			// Assert
			expectedHtml.ShouldBe(actualHtml);
		}

		[Fact]
		public void should_contain_empty_list_when_when_deserializing_bad_xml_file()
		{
			// Arrange
			TextSettings settings = new TextSettings();
			settings.CustomTokensPath = Path.Combine(Directory.GetCurrentDirectory(), "Text", "CustomTokens", "badxml-file.json");
			string expectedHtml = "@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@";

			// Act
			CustomTokenParser parser = new CustomTokenParser(settings, _logger);
			string actualHtml = parser.ReplaceTokensAfterParse("@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@");

			// Assert
			expectedHtml.ShouldBe(actualHtml);
		}

		[Fact]
		public void warningbox_token_should_return_html_fragment()
		{
			// Arrange
			TextSettings settings = new TextSettings();
			settings.CustomTokensPath = Path.Combine(Directory.GetCurrentDirectory(), "Text", "CustomTokens", "customvariables.xml");
			CustomTokenParser parser = new CustomTokenParser(settings, _logger);

			string expectedHtml = @"<div class=""alert alert-warning"">ENTER YOUR CONTENT HERE {{some link}}</div><br style=""clear:both""/>";

			// Act
			string actualHtml = parser.ReplaceTokensAfterParse("@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@");

			// Assert
			expectedHtml.ShouldBe(actualHtml);
		}
	}
}
