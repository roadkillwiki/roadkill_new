using Microsoft.AspNetCore.Mvc;
using Moq;
using Roadkill.Text;
using Roadkill.Text.Parsers.Images;
using Xunit;

namespace Roadkill.Tests.Unit.Text.Parsers.Images
{
    public class ImageSrcParserTests
    {
        private TextSettings _textSettings;
        private ImageSrcParser _srcParser;
        private Mock<IUrlHelper> _urlHelper;

        public ImageSrcParserTests()
        {
            _textSettings = new TextSettings();
            _urlHelper = new Mock<IUrlHelper>();

            _srcParser = new ImageSrcParser(_textSettings, _urlHelper.Object);
        }

        [Theory]
        [InlineData("www.foo.com/img.jpg")]
        [InlineData("http://www.example.com/img.jpg")]
        [InlineData("https://www.foo.com/img.jpg")]
        public void should_ignore_urls_starting_with_ww_http_and_https(string imageUrl)
        {
            // Arrange
            HtmlImageTag htmlImageTag = new HtmlImageTag(imageUrl, imageUrl, "alt", "title");

            // Act
            HtmlImageTag actualTag = _srcParser.Parse(htmlImageTag);

            // Assert
            Assert.Equal(imageUrl, actualTag.Src);
        }

        [Theory]
        [InlineData("/DSC001.jpg", "/attuchments/DSC001.jpg")]
        public void absolute_paths_should_be_prefixed_with_attachmentpath(string path, string expectedPath)
        {
            // Arrange
            _urlHelper.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(s => s);

            _textSettings.AttachmentsUrlPath = "/attuchments/";
            HtmlImageTag htmlImageTag = new HtmlImageTag(path, path, "alt", "title");

            // Act
            HtmlImageTag actualTag = _srcParser.Parse(htmlImageTag);

            // Assert
            Assert.Equal(expectedPath, actualTag.Src);
        }
    }
}