using System;
using System.Collections.Generic;
using System.Linq;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Roadkill.Text.Parsers.Images;
using Roadkill.Text.Parsers.Links;

namespace Roadkill.Text.Parsers.Markdig
{
	public class MarkdigImageAndLinkWalker
	{
		private readonly Action<HtmlImageTag> _imageDelegate;
		private readonly Action<HtmlLinkTag> _linkDelegate;

		public MarkdigImageAndLinkWalker(Action<HtmlImageTag> imageDelegate, Action<HtmlLinkTag> linkDelegate)
		{
			_imageDelegate = imageDelegate;
			_linkDelegate = linkDelegate;
		}

		public void WalkAndBindParseEvents(MarkdownObject markdownObject)
		{
			foreach (MarkdownObject child in markdownObject.Descendants())
			{
				// LinkInline can be both an <img.. or a <a href="...">
				LinkInline linkInline = child as LinkInline;
				if (linkInline != null)
				{
					EnsureAttributesInLink(linkInline);

					if (linkInline.IsImage)
					{
						string altText = "";

						var descendentForAltTag = child.Descendants().FirstOrDefault();
						if (descendentForAltTag != null)
						{
							altText = descendentForAltTag.ToString();
						}

						string title = altText;

						if (_imageDelegate != null)
						{
							HtmlImageTag args = InvokeImageParsedEvent(linkInline.Url, altText);

							if (!string.IsNullOrEmpty(args.Alt))
							{
								altText = args.Alt;
							}

							if (!string.IsNullOrEmpty(args.Title))
							{
								title = args.Title;
							}

							// Update the HTML from the data the event gives back
							linkInline.Url = args.Src;
						}

						// Replace to alt= attribute, it's a literal
						var literalInline = new LiteralInline(altText);
						linkInline.FirstChild.ReplaceBy(literalInline);

						// HTML5 the tag
						linkInline.Title = title;

						// Necessary for links and Bootstrap 3
						AddAttribute(linkInline, "border", "0");

						// Make all images expand via this Bootstrap class
						AddClass(linkInline, "img-responsive");
					}
					else
					{
						if (_linkDelegate != null)
						{
							string text = linkInline.Title;
							var descendentForAltTag = child.Descendants().FirstOrDefault();
							if (descendentForAltTag != null)
							{
								text = descendentForAltTag.ToString();
							}

							HtmlLinkTag args = InvokeLinkParsedEvent(linkInline.Url, text, linkInline.Label);

							// Update the HTML from the data the event gives back
							linkInline.Url = args.Href;

							if (!string.IsNullOrEmpty(args.Target))
							{
								AddAttribute(linkInline, "target", args.Target);
							}

							if (!string.IsNullOrEmpty(args.CssClass))
							{
								AddClass(linkInline, args.CssClass);
							}

							// Replace the link's text
							var literalInline = new LiteralInline(args.Text);
							linkInline.FirstChild.ReplaceBy(literalInline);
						}

						// Markdig TODO: make these configurable (external-links: [])
						if (!string.IsNullOrEmpty(linkInline.Url))
						{
							string upperUrl = linkInline.Url.ToUpperInvariant();
							if (upperUrl.StartsWith("HTTP://", StringComparison.Ordinal) ||
								upperUrl.StartsWith("HTTPS://", StringComparison.Ordinal) ||
								upperUrl.StartsWith("MAILTO:", StringComparison.Ordinal))
							{
								AddAttribute(linkInline, "rel", "nofollow");
							}
						}
					}
				}

				WalkAndBindParseEvents(child);
			}
		}

		private static void EnsureAttributesInLink(LinkInline link)
		{
			HtmlAttributes attributes = link.GetAttributes();
			if (attributes == null)
			{
				attributes = new HtmlAttributes();
				attributes.Classes = new List<string>();
			}

			if (attributes.Properties == null)
			{
				attributes.Properties = new List<KeyValuePair<string, string>>();
			}

			if (attributes.Classes == null)
			{
				attributes.Classes = new List<string>();
			}
		}

		private static void AddAttribute(LinkInline link, string name, string value)
		{
			HtmlAttributes attributes = link.GetAttributes();

			if (!attributes.Properties.Any(x => x.Key == name))
			{
				attributes.AddPropertyIfNotExist(name, value);
				link.SetAttributes(attributes);
			}
		}

		private static void AddClass(LinkInline link, string cssClass)
		{
			HtmlAttributes attributes = link.GetAttributes();

			if (!attributes.Classes.Any(x => x == cssClass))
			{
				attributes.Classes.Add(cssClass);
				link.SetAttributes(attributes);
			}
		}

		private HtmlImageTag InvokeImageParsedEvent(string url, string altText)
		{
			// Markdig TODO
			// string linkID = altText.ToLowerInvariant();
			HtmlImageTag args = new HtmlImageTag(url, url, altText, "");
			_imageDelegate(args);

			return args;
		}

		private HtmlLinkTag InvokeLinkParsedEvent(string url, string text, string target)
		{
			HtmlLinkTag args = new HtmlLinkTag(url, url, text, target);
			_linkDelegate(args);

			return args;
		}
	}
}
