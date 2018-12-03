using Microsoft.AspNetCore.Mvc;
using Moq;
using Roadkill.Text;
using Roadkill.Text.Parsers.Links;
using Roadkill.Text.Parsers.Links.Converters;
using Xunit;

namespace Roadkill.Tests.Unit.Text.Parsers.Links.Converters
{
    public class AttachmentLinkConverterTests
    {
        private TextSettings _textSettings;
        private Mock<IUrlHelper> _urlHelperMock;
        private AttachmentLinkConverter _converter;

        public AttachmentLinkConverterTests()
        {
            _textSettings = new TextSettings();
            _urlHelperMock = new Mock<IUrlHelper>();
            _converter = new AttachmentLinkConverter(_textSettings, _urlHelperMock.Object);
        }

        [Fact]
        public void IsMatch_should_return_false_for_null_link()
        {
            // Arrange
            HtmlLinkTag htmlTag = null;

            // Act
            bool actualMatch = _converter.IsMatch(htmlTag);

            // Assert
            Assert.False(actualMatch);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("http://www.google.com", false)]
        [InlineData("internal-link", false)]
        [InlineData("attachment:/foo/bar.jpg", true)]
        [InlineData("~/foo/bar.jpg", true)]
        public void IsMatch_should_match_attachment_links(string href, bool expectedMatch)
        {
            // Arrange
            var htmlTag = new HtmlLinkTag(href, href, "text", "");

            // Act
            bool actualMatch = _converter.IsMatch(htmlTag);

            // Assert
            Assert.Equal(actualMatch, expectedMatch);
        }

        [Theory]
        [InlineData("http://www.google.com", "http://www.google.com", false)]
        [InlineData("internal-link", "internal-link", false)]
        [InlineData("attachment:foo/bar.jpg", "/myattachments/foo/bar.jpg", true)]
        [InlineData("attachment:/foo/bar.jpg", "/myattachments/foo/bar.jpg", true)]
        [InlineData("~/foo/bar.jpg", "/myattachments/foo/bar.jpg", true)]
        public void Convert_should_change_expected_urls_to_full_paths(string href, string expectedHref, bool calledUrlHelper)
        {
            // Arrange
            _urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(s => s);

            _textSettings.AttachmentsUrlPath = "/myattachments/";
            var expectedTag = new HtmlLinkTag(href, href, "text", "");

            // Act
            var actualTag = _converter.Convert(expectedTag);

            // Assert
            Assert.Equal(expectedTag.OriginalHref, actualTag.OriginalHref);
            Assert.Equal(expectedHref, actualTag.Href);

            Times timesCalled = (calledUrlHelper) ? Times.Once() : Times.Never();
            _urlHelperMock.Verify(x => x.Content(It.IsAny<string>()), timesCalled);
        }
    }
}