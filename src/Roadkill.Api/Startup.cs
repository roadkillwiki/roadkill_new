using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Roadkill.Api.Extensions;
using Roadkill.Api.HealthChecks;
using Roadkill.Core.Configuration;
using Roadkill.Core.Extensions;
using Roadkill.Core.Settings;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace Roadkill.Api
{
	public class Startup
	{
	    private readonly ILoggerFactory _loggerFactory;

		private readonly IConfigurationRoot _configuration;

		private IHostingEnvironment _hostingEnvironment;

		public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;
			_hostingEnvironment = env;

			var builder = new ConfigurationBuilder();
			builder
				.SetBasePath(Path.Combine(env.ContentRootPath))
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
				.AddEnvironmentVariables();

			_configuration = builder.Build();
		}

	    public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			ILogger logger = _loggerFactory.CreateLogger("Startup");

			// Shared
			services.AddLogging();

			// Core
			services.ScanAndRegisterCore();
			var postgresSettings = services.AddConfigurationOf<PostgresSettings>(_configuration);
			services.AddConfigurationOf<SmtpSettings>(_configuration);
			services.AddMartenDocumentStore(postgresSettings.ConnectionString, logger);

			// API
			services.ScanAndRegisterApi();
			services.AddMailkit();
			services.AddMarkdown();
			services.AddJwtDefaults(_configuration, logger);
			services.AddIdentityDefaults();
			services.AddMvcAndVersionedSwagger();
			services.AddHealthChecks()
				.AddCheck<MartenHealthCheck>("marten");

			return services.BuildServiceProvider();
		}

	    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment environment)
		{
			// You can add these parameters to this method: IHostingEnvironment env, ILoggerFactory loggerFactory
			app.UseSwaggerUi3(settings => { settings.Path = ""; });
			app.UseSwagger();
			app.UseAuthentication();
			app.UseMvc();
			app.UseJsonExceptionHandler(environment);
			app.UseForwardedHeaders();
			app.UseJsonHealthChecks();
		}
	}
}
