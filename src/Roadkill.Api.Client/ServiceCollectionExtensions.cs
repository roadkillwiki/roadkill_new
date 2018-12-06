using System;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Roadkill.Api.Common.Services;

namespace Roadkill.Api.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRoadkillApi(this IServiceCollection services, string baseAddress, RefitSettings refitSettings = null)
        {
            if (refitSettings == null)
            {
                refitSettings = new RefitSettings();
            }

            services.AddRefitClient<IEmailService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

            services.AddRefitClient<IExportService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

            services.AddRefitClient<IFileService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

            services.AddRefitClient<IMarkdownService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

            services.AddRefitClient<IEmailService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

            services.AddRefitClient<IPagesService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

            services.AddRefitClient<IPageVersionsService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

            services.AddRefitClient<IEmailService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

            services.AddRefitClient<ISearchService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

            services.AddRefitClient<ITagsService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

            services.AddRefitClient<IUserService>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

            return services;
        }
    }
}
