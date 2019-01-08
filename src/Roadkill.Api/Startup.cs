using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Hellang.Middleware.ProblemDetails;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Roadkill.Api.Settings;
using Roadkill.Core.Configuration;
using Roadkill.Core.Settings;
using ApiDependencyInjection = Roadkill.Api.DependencyInjection;
using CoreDependencyInjection = Roadkill.Core.DependencyInjection;

namespace Roadkill.Api
{
	[SuppressMessage("Stylecop", "CA1822", Justification = "Methods cannot be static as they are used by the runtime")]
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
		    services.AddProblemDetails(x =>
		    {
		        x.IncludeExceptionDetails = context => _hostingEnvironment.IsDevelopment();
		        x.Map<Exception>(ex => new ExceptionProblemDetails(ex, StatusCodes.Status500InternalServerError));
		    });

			ILogger startupLogger = _loggerFactory.CreateLogger("Startup");
			services.AddLogging();

			// Settings
			var jwtSettings = new JwtSettings();
			_configuration.Bind("Jwt", jwtSettings);

			if (string.IsNullOrEmpty(jwtSettings.Password) || jwtSettings.Password.Length < 20)
			{
				startupLogger.LogError($"The JWT password '{jwtSettings.Password}' is empty or under 20 characters in length.");
				throw new InvalidOperationException("The JWT password is empty or under 20 characters in length.");
			}

			CoreDependencyInjection.GuardAllConfigProperties("Jwt", jwtSettings);

			// Roadkill IoC
			CoreDependencyInjection.ConfigureServices(services, _configuration, startupLogger);
			ApiDependencyInjection.ConfigureServices(services);

			// Authentication (ASP.NET Identity)
			ApiDependencyInjection.ConfigureIdentity(services);

			// Authorization (JWT)
			ApiDependencyInjection.ConfigureJwt(services, jwtSettings);

			services.AddMvc();
			services.AddSwaggerDocument();

			var provider = services.BuildServiceProvider();
			return provider;
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app)
		{
			// You can add these parameters to this method: IHostingEnvironment env, ILoggerFactory loggerFactory
			app.UseSwagger();
			app.UseSwaggerUi3();
			app.UseStaticFiles();
			app.UseAuthentication();
		    app.UseProblemDetails();
			app.UseMvc();
		}
	}
}
