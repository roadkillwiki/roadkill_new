using Roadkill.Text.Models;
using Roadkill.Text.Parsers;

namespace Roadkill.Text.TextMiddleware
{
    public class MarkupParserMiddleware : Middleware
    {
        private IMarkupParser _parser;

        public MarkupParserMiddleware(IMarkupParser parser)
        {
            _parser = parser;
        }

        public override PageHtml Invoke(PageHtml pageHtml)
        {
            pageHtml.Html = _parser.ToHtml(pageHtml.Html);
            return pageHtml;
        }
    }
}
