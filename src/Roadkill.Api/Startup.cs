using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roadkill.Api.Settings;
using ApiDependencyInjection = Roadkill.Api.DependencyInjection;
using CoreDependencyInjection = Roadkill.Core.DependencyInjection;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace Roadkill.Api
{
	[SuppressMessage("Stylecop", "CA1822", Justification = "Methods cannot be static as they are used by the runtime")]
	[SuppressMessage("ReSharper", "SA1118", Justification = "cos")]
	public class Startup
	{
	    private readonly ILoggerFactory _loggerFactory;

		private IConfigurationRoot _configuration;

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
			ILogger startupLogger = _loggerFactory.CreateLogger("Startup");
			services.AddLogging();

			// JWT password
			var jwtSettings = new JwtSettings();
			_configuration.Bind("Jwt", jwtSettings);
			CoreDependencyInjection.GuardAllConfigProperties("Jwt", jwtSettings);
			if (jwtSettings.Password.Length < 20)
			{
				startupLogger.LogError($"The JWT.Password is under 20 characters in length.");
				throw new InvalidOperationException("The JWT.Password setting is under 20 characters in length.");
			}

			if (jwtSettings.ExpiresDays < 1)
			{
				startupLogger.LogError($"The JWT.ExpiresDays is {jwtSettings.ExpiresDays}.");
				throw new InvalidOperationException("The JWT.ExpiresDays is setting should be 1 or greater.");
			}

			services.AddSingleton(jwtSettings);

			// Roadkill IoC
			CoreDependencyInjection.ConfigureServices(services, _configuration, startupLogger);
			ApiDependencyInjection.ConfigureServices(services);

			// Authentication (ASP.NET Identity)
			ApiDependencyInjection.ConfigureIdentity(services);

			// Authorization (JWT)
			ApiDependencyInjection.ConfigureJwt(services, jwtSettings);

			services
				.AddMvcCore()
				.AddDataAnnotations()
				.AddApiExplorer()
				.AddJsonFormatters()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
				.AddVersionedApiExplorer(options =>
				{
					options.SubstituteApiVersionInUrl = true;
					options.GroupNameFormat = "VVV";
				});

			services.AddApiVersioning(options =>
			{
				options.ApiVersionReader = new UrlSegmentApiVersionReader();
				options.AssumeDefaultVersionWhenUnspecified = true;
			});
			services.AddAllSwagger();

			var provider = services.BuildServiceProvider();
			return provider;
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app)
		{
			// You can add these parameters to this method: IHostingEnvironment env, ILoggerFactory loggerFactory
			app.UseSwaggerUi3();
			app.UseSwagger();
			app.UseAuthentication();
			app.UseMvc();
		}
	}
}
