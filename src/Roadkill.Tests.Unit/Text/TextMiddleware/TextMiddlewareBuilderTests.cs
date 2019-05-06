using System;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Roadkill.Text;
using Roadkill.Text.CustomTokens;
using Roadkill.Text.Models;
using Roadkill.Text.Plugins;
using Roadkill.Text.Sanitizer;
using Roadkill.Text.TextMiddleware;
using Xunit;

namespace Roadkill.Tests.Unit.Text.TextMiddleware
{
	public class TextMiddlewareBuilderTests
	{
		private readonly ILogger<TextMiddlewareBuilder> _logger;

		public TextMiddlewareBuilderTests()
		{
			_logger = Substitute.For<ILogger<TextMiddlewareBuilder>>();
		}

		[Fact]
		public void should()
		{
			// given
			var builder = CreateBuilderWithoutParser();

			// when
			PageHtml pageHtml = builder.Execute("![Image title](/DSC001.jpg)");

			// then
			Console.WriteLine(pageHtml);
		}

		[Fact]
		public void use_should_throw_when_middleware_is_null()
		{
			// given, when
			var builder = new TextMiddlewareBuilder(_logger);

			// then
			Assert.Throws<ArgumentNullException>(() => builder.Use(null));
		}

		[Fact]
		public void execute_should_swallow_exceptions()
		{
			// given
			string markup = "item1 item2";
			var builder = new TextMiddlewareBuilder(_logger);
			var middleware1 = new MiddleWareMock() { SearchString = null, Replacement = null };

			builder.Use(middleware1);

			// when
			string result = builder.Execute(markup);

			// then
			Assert.Equal("item1 item2", result);
		}

		[Fact]
		public void use_should_add_middleware_and_execute_should_concatenate_values_from_middleware()
		{
			// given
			string markup = "item1 item2";
			var builder = new TextMiddlewareBuilder(_logger);
			var middleware1 = new MiddleWareMock() { SearchString = "item1", Replacement = "value1" };
			var middleware2 = new MiddleWareMock() { SearchString = "item2", Replacement = "value2" };

			builder.Use(middleware1);
			builder.Use(middleware2);

			// when
			string result = builder.Execute(markup);

			// then
			Assert.Equal("value1 value2", result);
		}

		private TextMiddlewareBuilder CreateBuilderWithoutParser()
		{
			var builder = new TextMiddlewareBuilder(_logger);
			var settings = new TextSettings();
			var whiteListProvider = Substitute.For<IHtmlWhiteListProvider>();

			builder.Use(new CustomTokenMiddleware(new CustomTokenParser(settings, _logger)))
				.Use(new HarmfulTagMiddleware(new HtmlSanitizerFactory(settings, whiteListProvider)))
				.Use(new TextPluginAfterParseMiddleware(new TextPluginRunner()));

			return builder;
		}

		private class MiddleWareMock : Middleware
		{
			public string SearchString { get; set; }

			public string Replacement { get; set; }

			public override PageHtml Invoke(PageHtml pageHtml)
			{
				return pageHtml.Html.Replace(SearchString, Replacement, StringComparison.Ordinal);
			}
		}
	}
}
