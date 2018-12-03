using System.Collections.Generic;
using Ganss.XSS;
using Moq;
using Roadkill.Text;
using Roadkill.Text.Sanitizer;
using Xunit;

namespace Roadkill.Tests.Unit.Text.Sanitizer
{
    public class HtmlSanitizerFactoryTests
    {
        private HtmlSanitizerFactory CreateFactory(TextSettings textSettings = null, Mock<IHtmlWhiteListProvider> whiteListProviderMock = null)
        {
            if (textSettings == null)
            {
                textSettings = new TextSettings() { UseHtmlWhiteList = true };
            }

            if (whiteListProviderMock == null)
            {
                whiteListProviderMock = new Mock<IHtmlWhiteListProvider>();
                whiteListProviderMock.Setup(x => x.Deserialize()).Returns(new HtmlWhiteListSettings()
                {
                    AllowedElements = new List<string>(),
                    AllowedAttributes = new List<string>()
                });
            }

            return new HtmlSanitizerFactory(textSettings, whiteListProviderMock.Object);
        }

        [Fact]
        public void should_return_null_when_not_enabled()
        {
            // given
            var textSettings = new TextSettings() { UseHtmlWhiteList = false };
            HtmlSanitizerFactory factory = CreateFactory(textSettings);

            // when
            IHtmlSanitizer sanitizer = factory.CreateHtmlSanitizer();

            // then
            Assert.Null(sanitizer);
        }

        [Fact]
        public void should_configure_whitelist_for_sanitizer()
        {
            // given
            var whiteListSettings = new HtmlWhiteListSettings()
            {
                AllowedElements = new List<string> { "StarWarsMarquee" },
                AllowedAttributes = new List<string> { "cheesecake" }
            };

            var whiteListProviderMock = new Mock<IHtmlWhiteListProvider>();
            whiteListProviderMock.Setup(x => x.Deserialize()).Returns(whiteListSettings);

            HtmlSanitizerFactory factory = CreateFactory(null, whiteListProviderMock);

            // when
            IHtmlSanitizer sanitizer = factory.CreateHtmlSanitizer();

            // then
            Assert.NotNull(sanitizer);
            Assert.False(sanitizer.AllowDataAttributes);

            Assert.Contains("http", sanitizer.AllowedSchemes);
            Assert.Contains("https", sanitizer.AllowedSchemes);
            Assert.Contains("mailto", sanitizer.AllowedSchemes);

            Assert.Contains("StarWarsMarquee", sanitizer.AllowedTags);
            Assert.Contains("cheesecake", sanitizer.AllowedAttributes);
        }

        [Fact]
        public void should_configure_removing_attribute_event_to_ignore_special_tag()
        {
            // given
            HtmlSanitizerFactory factory = CreateFactory();
            const string expectedHtml = "<a href=\"Special:redpage\"></a>";

            // when
            IHtmlSanitizer sanitizer = factory.CreateHtmlSanitizer();

            // then
            string actualHtml = sanitizer.Sanitize(expectedHtml);

            Assert.Equal(expectedHtml, actualHtml);
        }
    }
}