using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Mime;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.Extensions;
using Roadkill.Api.HealthChecks;
using Roadkill.Core.Authorization;
using Roadkill.Core.Entities;
using Roadkill.Core.Extensions;
using Roadkill.Core.Settings;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace Roadkill.Api
{
	public class Startup : StartupBase
	{
		private readonly IConfiguration _configuration;

		private IHostingEnvironment _hostingEnvironment;

		private ILogger<Startup> _logger;

		public Startup(IHostingEnvironment env, ILogger<Startup> logger, IConfiguration configuration)
		{
			_logger = logger;
			_hostingEnvironment = env;
			_configuration = configuration;
		}

		public override void ConfigureServices(IServiceCollection services)
		{
			// Shared
			services.AddLogging();

			// Core
			services.ScanAndRegisterCore();
			var postgresSettings = services.AddConfigurationOf<PostgresSettings>(_configuration);
			services.AddConfigurationOf<SmtpSettings>(_configuration);
			services.AddMartenDocumentStore(postgresSettings.ConnectionString, _logger);

			// API
			services.ScanAndRegisterApi();
			services.AddAutoMapperForApi();
			services.AddMailkit();
			services.AddMarkdown();
			services.AddJwtDefaults(_configuration, _logger);
			services.AddIdentityDefaults();
			services.AddMvcAndVersionedSwagger();
			services.AddHealthChecks()
				.AddCheck<MartenHealthCheck>("marten");
		}

		public void ConfigureTestServices(IServiceCollection services)
		{
			// Configure services for Test environment
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		// You can add these parameters to this method: IHostingEnvironment env, ILoggerFactory loggerFactory
		public override void Configure(IApplicationBuilder app)
		{
			app.UseSwaggerWithReverseProxySupport();
			app.UseAuthentication();
			app.UseMvc();
			app.UseJsonExceptionHandler(_hostingEnvironment);
			app.UseForwardedHeaders();
			app.UseJsonHealthChecks();
		}
	}
}
