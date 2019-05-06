using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Roadkill.Text.Parsers.Links;
using Roadkill.Text.Parsers.Links.Converters;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Text.Parsers.Links.Converters
{
	public class SpecialLinkConverterTests
	{
		private IUrlHelper _urlHelperMock;
		private SpecialLinkConverter _converter;

		public SpecialLinkConverterTests()
		{
			_urlHelperMock = Substitute.For<IUrlHelper>();
			_converter = new SpecialLinkConverter(_urlHelperMock);
		}

		[Fact]
		public void IsMatch_should_return_false_for_null_link()
		{
			// Arrange
			HtmlLinkTag htmlTag = null;

			// Act
			bool actualMatch = _converter.IsMatch(htmlTag);

			// Assert
			actualMatch.ShouldBeFalse();
		}

		[Theory]
		[InlineData(null, false)]
		[InlineData("", false)]
		[InlineData("http://www.google.com", false)]
		[InlineData("internal-link", false)]
		[InlineData("special:MyPage", true)]
		public void IsMatch_should_match_attachment_links(string href, bool expectedMatch)
		{
			// Arrange
			var htmlTag = new HtmlLinkTag(href, href, "text", "");

			// Act
			bool actualMatch = _converter.IsMatch(htmlTag);

			// Assert
			actualMatch.ShouldBe(expectedMatch);
		}

		[Theory]
		[InlineData("http://www.google.com", "http://www.google.com", false)]
		[InlineData("internal-link", "internal-link", false)]
		[InlineData("special:foofoo", "~/wiki/special:foofoo", true)]
		public void Convert_should_change_expected_urls_to_full_paths(string href, string expectedHref, bool calledUrlHelper)
		{
			// Arrange
			_urlHelperMock
				.Content(Arg.Any<string>())
				.Returns<string>(callInfo => callInfo.Arg<string>());

			var originalTag = new HtmlLinkTag(href, href, "text", "");

			// Act
			var actualTag = _converter.Convert(originalTag);

			// Assert
			originalTag.OriginalHref.ShouldBe(actualTag.OriginalHref);
			expectedHref.ShouldBe(actualTag.Href);

			int timesCalled = (calledUrlHelper) ? 1 : 0;

			_urlHelperMock
				.Received(timesCalled)
				.Content(Arg.Any<string>());
		}
	}
}
