using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roadkill.Api.Extensions;
using Roadkill.Api.HealthChecks;
using Roadkill.Core.Extensions;
using Roadkill.Core.Settings;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace Roadkill.Api
{
	public class Startup : StartupBase
	{
		private readonly IConfiguration _configuration;
		private readonly IWebHostEnvironment _hostingEnvironment;
		private readonly ILogger<Startup> _logger;

		public Startup(IWebHostEnvironment env, ILogger<Startup> logger, IConfiguration configuration)
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
			services.AddCors(options =>
			{
					options.AddPolicy("localhost",
					builder =>
					{
						builder.WithOrigins("http://localhost:3000")
							.AllowAnyHeader()
							.AllowAnyMethod();
					});
			});
			services.AddAutoMapperForApi();
			services.AddMailkit();
			services.AddMarkdown();
			services.AddJwtDefaults(_configuration, _logger);
			services.AddIdentityDefaults();
			services.AddMvcAndVersionedSwagger();
			services
				.AddHealthChecks()
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
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseCors("localhost");

			app.UseSwaggerWithReverseProxySupport();

			app.UseEndpoints(endPoints =>
			{
				endPoints.MapControllers();
			});

			app.UseJsonExceptionHandler(_hostingEnvironment);
			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.All
			});
			app.UseJsonHealthChecks();
		}
	}
}
