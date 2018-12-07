using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Roadkill.Text;
using Roadkill.Text.Sanitizer;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Text.Sanitizer
{
	public class HtmlWhiteListProviderTests
	{
		private readonly TextSettings _textSettings;
		private HtmlWhiteListProvider _htmlWhiteListProvider;

		public HtmlWhiteListProviderTests()
		{
			var logger = Mock.Of<ILogger>();
			_textSettings = new TextSettings();
			_htmlWhiteListProvider = new HtmlWhiteListProvider(_textSettings, logger);
		}

		[Fact]
		public void should_return_whitelistsettings_from_derialized_file()
		{
			// given
			_textSettings.HtmlElementWhiteListPath = Path.Combine(Directory.GetCurrentDirectory(), "Text", "Sanitizer", "whitelist.json");

			// when
			HtmlWhiteListSettings settings = _htmlWhiteListProvider.Deserialize();

			// then
			settings.AllowedElements.Count.ShouldBe(2);
			settings.AllowedAttributes.Count.ShouldBe(3);

			settings.AllowedElements[0].ShouldBe("blah");
			settings.AllowedElements[1].ShouldBe("test");

			settings.AllowedAttributes[0].ShouldBe("id");
			settings.AllowedAttributes[1].ShouldBe("class");
			settings.AllowedAttributes[2].ShouldBe("href");
		}

		[Fact]
		public void should_return_default_whitelistsettings_when_path_is_empty()
		{
			// given
			var defaultSettings = _htmlWhiteListProvider.CreateDefaultWhiteList();

			// when
			HtmlWhiteListSettings settings = _htmlWhiteListProvider.Deserialize();

			// then
			defaultSettings.ShouldBeEquivalent(settings);
		}

		[Fact]
		public void should_return_default_whitelistsettings_when_whitelist_file_is_missing()
		{
			// given
			_textSettings.HtmlElementWhiteListPath = "file that doesnt exist.json";
			var defaultSettings = _htmlWhiteListProvider.CreateDefaultWhiteList();

			// when
			HtmlWhiteListSettings settings = _htmlWhiteListProvider.Deserialize();

			// then
			defaultSettings.ShouldBeEquivalent(settings);
		}

		[Fact]
		public void should_return_default_whitelistsettings_when_exception_occurs()
		{
			// given
			_textSettings.HtmlElementWhiteListPath = Path.Combine(Directory.GetCurrentDirectory(), "Text", "Sanitizer", "dodgy-whitelist.json");
			var defaultSettings = _htmlWhiteListProvider.CreateDefaultWhiteList();

			// when
			HtmlWhiteListSettings settings = _htmlWhiteListProvider.Deserialize();

			// then
			defaultSettings.ShouldBeEquivalent(settings);
		}
	}
}
