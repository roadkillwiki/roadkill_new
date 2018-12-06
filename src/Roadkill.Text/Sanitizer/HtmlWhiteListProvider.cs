using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Roadkill.Text.Sanitizer
{
    public interface IHtmlWhiteListProvider
    {
        HtmlWhiteListSettings Deserialize();
    }

    public class HtmlWhiteListProvider : IHtmlWhiteListProvider
    {
        private readonly TextSettings _textSettings;

        private readonly ILogger _logger;

        public HtmlWhiteListProvider(TextSettings settings, ILogger logger)
        {
            _textSettings = settings;
            _logger = logger;
        }

        public HtmlWhiteListSettings Deserialize()
        {
            if (string.IsNullOrEmpty(_textSettings.HtmlElementWhiteListPath) || !File.Exists(_textSettings.HtmlElementWhiteListPath))
            {
	            if (!string.IsNullOrEmpty(_textSettings.HtmlElementWhiteListPath))
	            {
		            _logger.LogWarning("The custom HTML white list tokens file does not exist in path '{0}' - using Default white list.", _textSettings.HtmlElementWhiteListPath);
	            }

                return CreateDefaultWhiteList();
            }

            try
            {
                string json = File.ReadAllText(_textSettings.HtmlElementWhiteListPath);
                var whiteList = JsonConvert.DeserializeObject<HtmlWhiteListSettings>(json);

	            if (whiteList == null)
	            {
		            return CreateDefaultWhiteList();
	            }

                return whiteList;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "An error occurred loading the html element white list file {0}", _textSettings.HtmlElementWhiteListPath);
                return CreateDefaultWhiteList();
            }
        }

        internal HtmlWhiteListSettings CreateDefaultWhiteList()
        {
            var allowedElements = new List<string>()
            {
                "strong",
                "b",
                "em",
                "i",
                "u",
                "strike",
                "sub",
                "sup",
                "p",
                "ol",
                "li",
                "ul",
                "font",
                "blockquote",
                "hr",
                "img",
                "div",
                "span",
                "br",
                "center",
                "a",
                "pre",
                "code",

                "h1",
                "h2",
                "h3",
                "h4",
                "h5",

                "table",
                "thead",
                "th",
                "tbody",
                "tr",
                "td"
            };

            var attributeList = new List<string>()
            {
                "style",
                "color",
                "face",
                "size",
                "src",
                "width",
                "height",
                "class",
                "align",
                "id",
                "dir",
                "rel"
            };

            return new HtmlWhiteListSettings()
            {
                AllowedElements = allowedElements,
                AllowedAttributes = attributeList
            };
        }
    }
}
