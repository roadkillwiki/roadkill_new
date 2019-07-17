using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Entities;
using Scrutor;

namespace Roadkill.Core.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection ScanAndRegisterCore(this IServiceCollection services)
		{
			services.Scan(scan => scan
				.FromAssemblyOf<Page>()
				.AddClasses()
				.UsingRegistrationStrategy(RegistrationStrategy.Skip)
				.AsMatchingInterface()
				.WithTransientLifetime());

			return services;
		}

		public static T AddConfigurationOf<T>(this IServiceCollection services, IConfiguration configuration)
			where T : class, new()
		{
			var settings = new T();
			string sectionName = typeof(T).Name.Replace("Settings", "");
			configuration.Bind(sectionName, settings);
			services.AddSingleton(settings);

			return settings;
		}
	}
}
