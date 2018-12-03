using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Roadkill.Text.Parsers.Images
{
    public class ImageSrcParser
    {
        private readonly TextSettings _textSettings;
        private static readonly Regex _imgFileRegex = new Regex("^File:", RegexOptions.IgnoreCase);

        private readonly IUrlHelper _urlHelper;

        public ImageSrcParser(TextSettings textSettings, IUrlHelper urlHelper)
        {
            if (textSettings == null)
                throw new ArgumentNullException(nameof(textSettings));

            _textSettings = textSettings;
            _urlHelper = urlHelper;
        }

        public HtmlImageTag Parse(HtmlImageTag htmlImageTag)
        {
            if (htmlImageTag.OriginalSrc.StartsWith("http://") || htmlImageTag.OriginalSrc.StartsWith("https://") ||
                htmlImageTag.OriginalSrc.StartsWith("www."))
            {
                return htmlImageTag;
            }

            // Adds the attachments folder as a prefix to all image URLs before the HTML img tag is written.
            string src = htmlImageTag.OriginalSrc;
            src = _imgFileRegex.Replace(src, "");

            string attachmentsPath = _textSettings.AttachmentsUrlPath;
            if (attachmentsPath.EndsWith("/"))
                attachmentsPath = attachmentsPath.Remove(attachmentsPath.Length - 1);

            string slash = src.StartsWith("/") ? "" : "/";
            string relativeUrl = attachmentsPath + slash + src;
            htmlImageTag.Src = _urlHelper.Content(relativeUrl); // convert to absolute path

            return htmlImageTag;
        }
    }
}