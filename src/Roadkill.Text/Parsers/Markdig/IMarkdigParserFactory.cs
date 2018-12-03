using Microsoft.AspNetCore.Mvc;
using Roadkill.Core.Repositories;

namespace Roadkill.Text.Parsers.Markdig
{
    public interface IMarkdigParserFactory
    {
        // TODO: NETStandard - replace urlhelper to IUrlHelper

        MarkdigParser Create(IPageRepository pageRepository, TextSettings textSettings, IUrlHelper urlHelper);
    }
}