using Microsoft.AspNetCore.Mvc;
using Roadkill.Core.Repositories;
using Roadkill.Text.Parsers.Images;
using Roadkill.Text.Parsers.Links;

namespace Roadkill.Text.Parsers.Markdig
{
    public class MarkdigParserFactory : IMarkdigParserFactory
    {
        public MarkdigParser Create(IPageRepository pageRepository, TextSettings textSettings, IUrlHelper urlHelper)
        {
            var markdigParser = new MarkdigParser();

            // When a link is parsed, use the LinkHrefParser
            markdigParser.LinkParsed = htmlLinkTag =>
            {
                var tokenParser = new LinkHrefParser(pageRepository, textSettings, urlHelper);
                htmlLinkTag = tokenParser.Parse(htmlLinkTag);

                return htmlLinkTag;
            };

            // When an image is parsed, use the ImageSrcParser
            markdigParser.ImageParsed = htmlImageTag =>
            {
                var provider = new ImageSrcParser(textSettings, urlHelper);
                htmlImageTag = provider.Parse(htmlImageTag);

                return htmlImageTag;
            };

            return markdigParser;
        }
    }
}
