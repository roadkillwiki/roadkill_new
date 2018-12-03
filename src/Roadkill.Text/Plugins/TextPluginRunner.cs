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

        public string BeforeParse(string text, PageHtml pageHtml)
        {
            return text;
        }

        public string AfterParse(string html)
        {
            return html;
        }

        public string PreContainerHtml()
        {
            return "";
        }

        public string PostContainerHtml()
        {
            return "";
        }
    }
}