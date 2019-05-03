using System;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Roadkill.Api.Client
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddRoadkillClient(this IServiceCollection services, string baseAddress, RefitSettings refitSettings = null)
		{
			if (refitSettings == null)
			{
				refitSettings = new RefitSettings();
			}

			services.AddRefitClient<IEmailClient>(refitSettings)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			services.AddRefitClient<IExportClient>(refitSettings)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			services.AddRefitClient<IFileClient>(refitSettings)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			services.AddRefitClient<IMarkdownClient>(refitSettings)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			services.AddRefitClient<IEmailClient>(refitSettings)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			services.AddRefitClient<IPagesClient>(refitSettings)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			services.AddRefitClient<IPageVersionsClient>(refitSettings)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			services.AddRefitClient<IEmailClient>(refitSettings)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			services.AddRefitClient<ISearchClient>(refitSettings)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			services.AddRefitClient<ITagsClient>(refitSettings)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			services.AddRefitClient<IUserClient>(refitSettings)
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			return services;
		}
	}
}
