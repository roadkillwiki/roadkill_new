using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Roadkill.Text.CustomTokens;
using Roadkill.Text.Models;
using Roadkill.Text.Parsers.Markdig;
using Roadkill.Text.Plugins;
using Roadkill.Text.Sanitizer;

namespace Roadkill.Text.TextMiddleware
{
	public interface ITextMiddlewareBuilder
	{
		List<Middleware> MiddlewareItems { get; }

		TextMiddlewareBuilder Use(Middleware middleware);

		PageHtml Execute(string markdown);
	}

	public class TextMiddlewareBuilder : ITextMiddlewareBuilder
	{
		private readonly ILogger _logger;

		public TextMiddlewareBuilder(ILogger logger)
		{
			_logger = logger;
			MiddlewareItems = new List<Middleware>();
		}

		public List<Middleware> MiddlewareItems { get; }

		public static TextMiddlewareBuilder Default(TextSettings textSettings, ILogger logger)
		{
			var whiteListProvider = new HtmlWhiteListProvider(textSettings, logger);
			var builder = new TextMiddlewareBuilder(logger);

			builder.Use(new CustomTokenMiddleware(new CustomTokenParser(textSettings, logger)))
				   .Use(new MarkupParserMiddleware(new MarkdigParser()))
				   .Use(new HarmfulTagMiddleware(new HtmlSanitizerFactory(textSettings, whiteListProvider)))
				   .Use(new TextPluginAfterParseMiddleware(new TextPluginRunner()));

			return builder;
		}

		public TextMiddlewareBuilder Use(Middleware middleware)
		{
			if (middleware == null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }

            MiddlewareItems.Add(middleware);
			return this;
		}

		public PageHtml Execute(string markdown)
		{
			var pageHtml = new PageHtml() { Html = markdown };

			foreach (Middleware item in MiddlewareItems)
			{
				try
				{
					pageHtml = item.Invoke(pageHtml);
				}
				catch (Exception ex)
				{
					_logger.LogWarning("TextMiddlewareBuilder exception: {0}", ex);
				}
			}

			return pageHtml;
		}
	}
}
