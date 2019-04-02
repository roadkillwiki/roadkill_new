using System;
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
			{
				return false;
			}

			if (string.IsNullOrEmpty(htmlLinkTag.OriginalHref))
			{
				return false;
			}

			string upperHref = htmlLinkTag.OriginalHref.ToUpperInvariant();
			return upperHref.StartsWith("ATTACHMENT:", StringComparison.Ordinal) || upperHref.StartsWith("~/", StringComparison.Ordinal);
		}

		public HtmlLinkTag Convert(HtmlLinkTag htmlLinkTag)
		{
			string href = htmlLinkTag.OriginalHref;
			string upperHref = href?.ToUpperInvariant() ?? "";

			if (!upperHref.StartsWith("ATTACHMENT:", StringComparison.Ordinal) &&
				!upperHref.StartsWith("~", StringComparison.Ordinal))
			{
				return htmlLinkTag;
			}

			if (upperHref.StartsWith("ATTACHMENT:", StringComparison.Ordinal))
			{
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

			htmlLinkTag.Href = _urlHelper.Content(attachmentsPath) + href;

			return htmlLinkTag;
		}
	}
}
