using System.Collections.Generic;

namespace Roadkill.Text.Sanitizer
{
    public class HtmlWhiteListSettings
    {
        public List<string> AllowedElements { get; set; }
        public List<string> AllowedAttributes { get; set; }
    }
}