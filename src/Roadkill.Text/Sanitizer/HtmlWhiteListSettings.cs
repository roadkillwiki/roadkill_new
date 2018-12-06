using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Roadkill.Text.Sanitizer
{
    [SuppressMessage("ReSharper", "CA2227", Justification = "Setter is required")]
    public class HtmlWhiteListSettings
    {
        public List<string> AllowedElements { get; set; }

        public List<string> AllowedAttributes { get; set; }
    }
}
