using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Roadkill.Text.Parsers.Images
{
    public class ImageSrcParser
    {
        private static readonly Regex _imgFileRegex = new Regex("^File:", RegexOptions.IgnoreCase);

	    private readonly TextSettings _textSettings;

        private readonly IUrlHelper _urlHelper;

        public ImageSrcParser(TextSettings textSettings, IUrlHelper urlHelper)
        {
	        _textSettings = textSettings ?? throw new ArgumentNullException(nameof(textSettings));
            _urlHelper = urlHelper;
        }

        public HtmlImageTag Parse(HtmlImageTag htmlImageTag)
        {
            if (htmlImageTag.OriginalSrc.StartsWith("http://", StringComparison.Ordinal) ||
                htmlImageTag.OriginalSrc.StartsWith("https://", StringComparison.Ordinal) ||
                htmlImageTag.OriginalSrc.StartsWith("www.", StringComparison.Ordinal))
            {
                return htmlImageTag;
            }

            // Adds the attachments folder as a prefix to all image URLs before the HTML img tag is written.
            string src = htmlImageTag.OriginalSrc;
            src = _imgFileRegex.Replace(src, "");

            string attachmentsPath = _textSettings.AttachmentsUrlPath;
            if (attachmentsPath.EndsWith("/", StringComparison.Ordinal))
            {
                attachmentsPath = attachmentsPath.Remove(attachmentsPath.Length - 1);
            }

            string slash = src.StartsWith("/", StringComparison.Ordinal) ? "" : "/";
            string relativeUrl = attachmentsPath + slash + src;
            htmlImageTag.Src = _urlHelper.Content(relativeUrl); // convert to absolute path

            return htmlImageTag;
        }
    }
}
