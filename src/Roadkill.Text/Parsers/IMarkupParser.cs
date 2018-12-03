using System;
using Roadkill.Text.Parsers.Images;
using Roadkill.Text.Parsers.Links;

namespace Roadkill.Text.Parsers
{
	/// <summary>
	/// Represents a class that can convert a markup syntax into HTML. The markups syntax 
	/// should include formatting support as well as images and links.
	/// </summary>
	public interface IMarkupParser
	{
		/// <summary>
		/// Transforms the provided specific markup text to HTML
		/// </summary>
		string ToHtml(string markdown);

        /// <summary>
        /// Callback that's called when an image tag is parsed.
        /// </summary>
	    Func<HtmlImageTag, HtmlImageTag> ImageParsed { get; set; }

        /// <summary>
        /// Callback when a link tag is parsed.
        /// </summary>
        Func<HtmlLinkTag, HtmlLinkTag> LinkParsed { get; set; }
	}
}
