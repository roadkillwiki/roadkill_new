using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Roadkill.Text;
using Roadkill.Text.Parsers.Images;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Text.Parsers.Images
{
	public class ImageSrcParserTests
	{
		private TextSettings _textSettings;
		private ImageSrcParser _srcParser;
		private IUrlHelper _urlHelper;

		public ImageSrcParserTests()
		{
			_textSettings = new TextSettings();
			_urlHelper = Substitute.For<IUrlHelper>();

			_srcParser = new ImageSrcParser(_textSettings, _urlHelper);
		}

		[Theory]
		[InlineData("www.foo.com/img.jpg")]
		[InlineData("http://www.example.com/img.jpg")]
		[InlineData("https://www.foo.com/img.jpg")]
		[SuppressMessage("Stylecop", "CA1054", Justification = "It's ok to not use a URI, I said so.")]
		public void should_ignore_urls_starting_with_ww_http_and_https(string imageUrl)
		{
			// Arrange
			HtmlImageTag htmlImageTag = new HtmlImageTag(imageUrl, imageUrl, "alt", "title");

			// Act
			HtmlImageTag actualTag = _srcParser.Parse(htmlImageTag);

			// Assert
			imageUrl.ShouldBe(actualTag.Src);
		}

		[Theory]
		[InlineData("/DSC001.jpg", "/attuchments/DSC001.jpg")]
		public void absolute_paths_should_be_prefixed_with_attachmentpath(string path, string expectedPath)
		{
			// Arrange
			_urlHelper
				.Content(Arg.Any<string>())
				.Returns(callInfo => callInfo.Arg<string>());

			_textSettings.AttachmentsUrlPath = "/attuchments/";
			HtmlImageTag htmlImageTag = new HtmlImageTag(path, path, "alt", "title");

			// Act
			HtmlImageTag actualTag = _srcParser.Parse(htmlImageTag);

			// Assert
			expectedPath.ShouldBe(actualTag.Src);
		}
	}
}
