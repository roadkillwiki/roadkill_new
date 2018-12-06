using Roadkill.Text.Models;

namespace Roadkill.Text.Plugins
{
    /// <summary>
    /// Runs the BeforeParse and AfterParse methods on all TextPlugins, and determines if
    /// the the HTML can be cached or not based on the plugins run.
    /// </summary>
    public class TextPluginRunner
    {
        public bool IsCacheable { get; set; }

        public static string BeforeParse(PageHtml pageHtml)
        {
            return pageHtml.Html;
        }

        public static string PostContainerHtml()
        {
            return "";
        }

	    public static string AfterParse(string html)
	    {
		    return html;
	    }

	    public static string PreContainerHtml()
	    {
		    return "";
	    }
    }
}
