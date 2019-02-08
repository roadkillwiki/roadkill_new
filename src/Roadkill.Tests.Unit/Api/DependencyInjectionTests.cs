using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roadkill.Api.Controllers;
using Roadkill.Text.Sanitizer;
using Roadkill.Text.TextMiddleware;
using Shouldly;
using Xunit;
using DependencyInjection = Roadkill.Api.DependencyInjection;

namespace Roadkill.Tests.Unit.Api
{
	public class DependencyInjectionTests
	{
		[Fact]
		public void ConfigureServices_should_register_known_types()
		{
			// given
			var services = new ServiceCollection();

			// when
			DependencyInjection.ConfigureServices(services);

			// then
			ServiceProvider provider = services.BuildServiceProvider();

			AssertInstanceIsNotNull<ILogger<AuthorizationController>>(provider);

			AssertInstanceIsNotNull<IMailTransport>(provider);
			AssertInstanceIsOfType<IMailTransport, SmtpClient>(provider);

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
