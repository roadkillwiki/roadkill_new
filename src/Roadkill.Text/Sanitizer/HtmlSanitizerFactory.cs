using Ganss.XSS;

namespace Roadkill.Text.Sanitizer
{
    public interface IHtmlSanitizerFactory
    {
        IHtmlSanitizer CreateHtmlSanitizer();
    }

    public class HtmlSanitizerFactory : IHtmlSanitizerFactory
    {
        private readonly TextSettings _textSettings;
        private readonly IHtmlWhiteListProvider _htmlWhiteListProvider;

        public HtmlSanitizerFactory(TextSettings textSettings, IHtmlWhiteListProvider htmlWhiteListProvider)
        {
            _textSettings = textSettings;
            _htmlWhiteListProvider = htmlWhiteListProvider;
        }

        public IHtmlSanitizer CreateHtmlSanitizer()
        {
            if (!_textSettings.UseHtmlWhiteList)
            {
                return null;
            }

            HtmlWhiteListSettings whiteListSettings = _htmlWhiteListProvider.Deserialize();
            string[] allowedTags = whiteListSettings.AllowedElements.ToArray();
            string[] allowedAttributes = whiteListSettings.AllowedAttributes.ToArray();

            if (allowedTags.Length == 0)
            {
                allowedTags = null;
            }

            if (allowedAttributes.Length == 0)
            {
                allowedAttributes = null;
            }

            var htmlSanitizer = new HtmlSanitizer(allowedTags, null, allowedAttributes);
            htmlSanitizer.AllowDataAttributes = false;
            htmlSanitizer.AllowedAttributes.Add("class");
            htmlSanitizer.AllowedAttributes.Add("id");
            htmlSanitizer.AllowedSchemes.Add("mailto");
            htmlSanitizer.RemovingAttribute += (sender, e) =>
            {
                // Don't clean /wiki/Special:Tag urls in href="" attributes
                if (e.Attribute.Name.ToUpperInvariant() == "HREF" && e.Attribute.Value.Contains("Special:"))
                {
                    e.Cancel = true;
                }
            };

            return htmlSanitizer;
        }
    }
}
