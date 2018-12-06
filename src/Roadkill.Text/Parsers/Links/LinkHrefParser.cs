using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;

namespace Roadkill.Text.Parsers.Links
{
    /// <summary>
    /// Parses:
    ///   - Tokens inside links, e.g. special:, attachment:
    ///   - Internal links.
    ///   - CSS classes for external.
    ///   - Broken links.
    /// </summary>
    public class LinkHrefParser
    {
	    private static readonly Regex _querystringRegex = new Regex("(?<querystring>(\\?).+)", RegexOptions.IgnoreCase);

	    private static readonly Regex _anchorRegex = new Regex("(?<hash>(#|%23).+)", RegexOptions.IgnoreCase);

        private readonly IPageRepository _pageRepository;

        private readonly TextSettings _textSettings;

        // TODO: NETStandard - replace urlhelper to IUrlHelper
        private readonly IUrlHelper _urlHelper;

        private readonly List<string> _externalLinkPrefixes = new List<string>()
        {
            "http://",
            "https://",
            "www.",
            "mailto:",
            "#",
            "tag:"
        };

        public LinkHrefParser(IPageRepository pageRepository, TextSettings textSettings, IUrlHelper urlHelper)
        {
	        _pageRepository = pageRepository ?? throw new ArgumentNullException(nameof(pageRepository));
            _textSettings = textSettings ?? throw new ArgumentNullException(nameof(textSettings));
            _urlHelper = urlHelper;
        }

        public HtmlLinkTag Parse(HtmlLinkTag htmlLinkTag)
        {
            if (IsExternalLink(htmlLinkTag.OriginalHref))
            {
                // Add the external-link class to all outward bound links,
                // except for anchors pointing to <a name=""> tags on the current page.
                // (# links shouldn't be treated as internal links)
                if (!htmlLinkTag.OriginalHref.StartsWith("#", StringComparison.Ordinal))
                {
                    htmlLinkTag.CssClass = "external-link";
                }
            }
            else
            {
                string href = htmlLinkTag.OriginalHref;
                string upperHref = href.ToUpperInvariant();

                if (upperHref.StartsWith("ATTACHMENT:", StringComparison.Ordinal) ||
                    upperHref.StartsWith("~/", StringComparison.Ordinal))
                {
                    ConvertAttachmentToFullPath(htmlLinkTag);
                }
                else if (upperHref.StartsWith("SPECIAL:", StringComparison.Ordinal))
                {
                    ConvertSpecialLinkToFullPath(htmlLinkTag);
                }
                else
                {
                    ConvertInternalLinkToFullPath(htmlLinkTag);
                }
            }

            return htmlLinkTag;
        }

	    // Removes all bad characters (ones which cannot be used in a URL for a page) from a page title.
	    private static string EncodePageTitle(string title)
	    {
		    if (string.IsNullOrEmpty(title))
            {
                return title;
            }

            // Search engine friendly slug routine with help from http://www.intrepidstudios.com/blog/2009/2/10/function-to-generate-a-url-friendly-string.aspx

            // remove invalid characters
            title = Regex.Replace(title, @"[^\w\d\s-]", "");  // this is unicode safe, but may need to revert back to 'a-zA-Z0-9', need to check spec

		    // convert multiple spaces/hyphens into one space
		    title = Regex.Replace(title, @"[\s-]+", " ").Trim();

		    // If it's over 30 chars, take the first 30.
		    title = title.Substring(0, title.Length <= 75 ? title.Length : 75).Trim();

		    // hyphenate spaces
		    title = Regex.Replace(title, @"\s", "-");

		    return title;
	    }

        private bool IsExternalLink(string href)
        {
            return _externalLinkPrefixes.Any(x => href.StartsWith(x, StringComparison.Ordinal));
        }

        private void ConvertAttachmentToFullPath(HtmlLinkTag htmlLinkTag)
        {
            string href = htmlLinkTag.OriginalHref;
	        string upperHref = href.ToUpperInvariant();

            if (upperHref.StartsWith("ATTACHMENT:", StringComparison.Ordinal))
            {
                // Remove the attachment: part
                href = href.Remove(0, 11);
                if (!href.StartsWith("/", StringComparison.Ordinal))
                {
                    href = "/" + href;
                }
            }
            else if (upperHref.StartsWith("~/", StringComparison.Ordinal))
            {
                // Remove the ~
                href = href.Remove(0, 1);
            }

            // Get the full path to the attachment
            string attachmentsPath = _textSettings.AttachmentsUrlPath;
            if (attachmentsPath.EndsWith("/", StringComparison.Ordinal))
            {
                attachmentsPath = attachmentsPath.Remove(attachmentsPath.Length - 1);
            }

            htmlLinkTag.Href = ConvertToAbsolutePath(attachmentsPath) + href;
        }

        private void ConvertSpecialLinkToFullPath(HtmlLinkTag htmlLinkTag)
        {
            // note: "~" is replaced by ASP.NET HttpContext
            string href = htmlLinkTag.OriginalHref;
            htmlLinkTag.Href = ConvertToAbsolutePath("~/wiki/" + href);
        }

        private string ConvertToAbsolutePath(string relativeUrl)
        {
            // e.g. ~/mydir/page1.html to /mywiki/mydir/page1.html.
            return _urlHelper.Content(relativeUrl);
        }

        private void ConvertInternalLinkToFullPath(HtmlLinkTag htmlLinkTag)
        {
            string href = htmlLinkTag.OriginalHref;

            // Parse internal links
            string title = href;
            string querystringAndAnchor = ""; // querystrings, #anchors

            // Parse querystrings and #
            if (_querystringRegex.IsMatch(href))
            {
                // Grab the querystring contents
                Match match = _querystringRegex.Match(href);
                querystringAndAnchor = match.Groups["querystring"].Value;

                // Grab the url
                title = href.Replace(querystringAndAnchor, "");
            }
            else if (_anchorRegex.IsMatch(href))
            {
                // Grab the hash contents
                System.Text.RegularExpressions.Match match = _anchorRegex.Match(href);
                querystringAndAnchor = match.Groups["hash"].Value;

                // Grab the url
                title = href.Replace(querystringAndAnchor, "");
            }

            // For markdown, only urls with "-" in them are valid, spaces are ignored.
            // Remove these, so a match is made. No page title should have a "-" in?, so replacing them is ok.
            title = title.Replace("-", " ");

            // Find the page, or if it doesn't exist point to the new page url
            Page page = _pageRepository.GetPageByTitle(title).GetAwaiter().GetResult();
            if (page != null)
            {
                string encodedTitle = EncodePageTitle(page.Title);
                UrlActionContext actionContext = new UrlActionContext()
                {
                    Action = "Index",
                    Controller = "Wiki",
                    Values = new { id = page.Id, title = encodedTitle }
                };

                href = _urlHelper.Action(actionContext);
                href += querystringAndAnchor;
            }
            else
            {
                // e.g. /pages/new?title=xyz
                UrlActionContext actionContext = new UrlActionContext()
                {
                    Action = "New",
                    Controller = "Pages",
                    Values = new { title = title }
                };
                href = _urlHelper.Action(actionContext);
                htmlLinkTag.CssClass = "missing-page-link";
            }

            htmlLinkTag.Href = href;
            htmlLinkTag.Target = "";
        }
    }
}
