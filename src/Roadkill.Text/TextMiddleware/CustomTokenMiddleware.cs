using Roadkill.Text.CustomTokens;
using Roadkill.Text.Models;

namespace Roadkill.Text.TextMiddleware
{
    public class CustomTokenMiddleware : Middleware
    {
        private readonly CustomTokenParser _customTokenParser;

        public CustomTokenMiddleware(CustomTokenParser customTokenParser)
        {
            _customTokenParser = customTokenParser;
        }

        public override PageHtml Invoke(PageHtml pageHtml)
        {
            pageHtml.Html = _customTokenParser.ReplaceTokensAfterParse(pageHtml.Html);
            return pageHtml;
        }
    }
}
