using System;
using Microsoft.AspNetCore.Mvc;

namespace Roadkill.Text.Parsers.Links.Converters
{
	public class SpecialLinkConverter : IHtmlLinkTagConverter
	{
		private readonly IUrlHelper _urlHelper;

		public SpecialLinkConverter(IUrlHelper urlHelper)
		{
			_urlHelper = urlHelper;
		}

		public bool IsMatch(HtmlLinkTag htmlLinkTag)
		{
			if (htmlLinkTag == null)
			{
				return false;
			}

			string href = htmlLinkTag.OriginalHref;
			string upperHref = href?.ToUpperInvariant() ?? "";

			return upperHref.StartsWith("SPECIAL:", StringComparison.Ordinal);
		}

		public HtmlLinkTag Convert(HtmlLinkTag htmlLinkTag)
		{
			string href = htmlLinkTag.OriginalHref;
			string upperHref = href?.ToUpperInvariant();

			if (!upperHref.StartsWith("SPECIAL:", StringComparison.Ordinal))
			{
				return htmlLinkTag;
			}

			htmlLinkTag.Href = _urlHelper.Content("~/wiki/" + href);

			return htmlLinkTag;
		}
	}
}
