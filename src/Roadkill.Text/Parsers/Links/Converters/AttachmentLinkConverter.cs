using Microsoft.AspNetCore.Mvc;

namespace Roadkill.Text.Parsers.Links.Converters
{
    public class AttachmentLinkConverter : IHtmlLinkTagConverter
    {
        private readonly TextSettings _textSettings;

        private readonly IUrlHelper _urlHelper;

        public AttachmentLinkConverter(TextSettings textSettings, IUrlHelper urlHelper)
        {
            _textSettings = textSettings;
            _urlHelper = urlHelper;
        }

        public bool IsMatch(HtmlLinkTag htmlLinkTag)
        {
            if (htmlLinkTag == null)
                return false;

            if (string.IsNullOrEmpty(htmlLinkTag.OriginalHref))
                return false;

            string lowerHref = htmlLinkTag.OriginalHref.ToLower();
            return lowerHref.StartsWith("attachment:") || lowerHref.StartsWith("~/");
        }

        public HtmlLinkTag Convert(HtmlLinkTag htmlLinkTag)
        {
            string href = htmlLinkTag.OriginalHref;
            string lowerHref = href?.ToLower() ?? "";

            if (!lowerHref.StartsWith("attachment:") && !lowerHref.StartsWith("~"))
                return htmlLinkTag;

            if (lowerHref.StartsWith("attachment:"))
            {
                href = href.Remove(0, 11);
                if (!href.StartsWith("/"))
                    href = "/" + href;
            }
            else if (lowerHref.StartsWith("~/"))
            {
                // Remove the ~
                href = href.Remove(0, 1);
            }

            // Get the full path to the attachment
            string attachmentsPath = _textSettings.AttachmentsUrlPath;
            if (attachmentsPath.EndsWith("/"))
                attachmentsPath = attachmentsPath.Remove(attachmentsPath.Length - 1);

            htmlLinkTag.Href = _urlHelper.Content(attachmentsPath) + href;

            return htmlLinkTag;
        }
    }
}