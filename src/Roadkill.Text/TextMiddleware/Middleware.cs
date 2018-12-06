using Roadkill.Text.Models;

namespace Roadkill.Text.TextMiddleware
{
    public abstract class Middleware
    {
        public abstract PageHtml Invoke(PageHtml pageHtml);
    }
}
