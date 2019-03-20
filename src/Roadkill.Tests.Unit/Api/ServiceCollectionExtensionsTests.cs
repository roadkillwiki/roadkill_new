using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Api.Extensions;
using Roadkill.Text;
using Roadkill.Text.Sanitizer;
using Roadkill.Text.TextMiddleware;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api
{
	// JWT, Identity and Swagger/MVC tests ?
	public class ServiceCollectionExtensionsTests
	{
		[Fact]
		public void should_register_mailkit()
		{
			// given
			var services = new ServiceCollection();

			// when
			services.AddMailkit();

			// then
			ServiceProvider provider = services.BuildServiceProvider();
			AssertInstanceIsNotNull<IMailTransport>(provider);
			AssertInstanceIsOfType<IMailTransport, SmtpClient>(provider);
		}

		[Fact]
		public void should_register_markdown_types()
		{
			// given
			var services = new ServiceCollection();
			services.AddLogging();

			// when
			services.AddMarkdown();

			// then
			ServiceProvider provider = services.BuildServiceProvider();
			AssertInstanceIsNotNull<TextSettings>(provider);
			AssertInstanceIsNotNull<IHtmlWhiteListProvider>(provider);
			AssertInstanceIsOfType<IHtmlWhiteListProvider, HtmlWhiteListProvider>(provider);
			AssertInstanceIsNotNull<ITextMiddlewareBuilder>(provider);
		}

		private static void AssertInstanceIsNotNull<TService>(ServiceProvider provider)
		{
			var instance = provider.GetService<TService>();
			instance.ShouldNotBeNull($"AssertType failed: {typeof(TService).Name} returned null");
		}

		private static void AssertInstanceIsOfType<TService, TInstance>(ServiceProvider provider)
		{
			var instance = provider.GetService<TService>();
			instance.ShouldBeOfType<TInstance>($"AssertType failed: {typeof(TService).Name} didn't give back a {typeof(TInstance).Name}");
		}
	}
}
