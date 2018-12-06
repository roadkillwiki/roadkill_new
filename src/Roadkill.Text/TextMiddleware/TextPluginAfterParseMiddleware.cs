using Roadkill.Text.Models;
using Roadkill.Text.Plugins;

namespace Roadkill.Text.TextMiddleware
{
    public class TextPluginAfterParseMiddleware : Middleware
    {
        private readonly TextPluginRunner _textPluginRunner;

        public TextPluginAfterParseMiddleware(TextPluginRunner textPluginRunner)
        {
            _textPluginRunner = textPluginRunner;
        }

        public override PageHtml Invoke(PageHtml pageHtml)
        {
            pageHtml.Html = TextPluginRunner.AfterParse(pageHtml.Html);
            pageHtml.PreContainerHtml = TextPluginRunner.PreContainerHtml();
            pageHtml.PostContainerHtml = TextPluginRunner.PostContainerHtml();
            pageHtml.IsCacheable = _textPluginRunner.IsCacheable;

            return pageHtml;
        }
    }
}
