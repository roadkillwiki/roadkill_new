using System;
using System.IO;
using System.Net;
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
using Roadkill.Core.Configuration;

using ApiDependencyInjection = Roadkill.Api.DependencyInjection;
using CoreDependencyInjection = Roadkill.Core.DependencyInjection;

namespace Roadkill.Api
{
	public class Startup
	{
	    private IConfigurationRoot _configuration { get; set; }
	    private IHostingEnvironment _hostingEnvironment { get; set; }
	    private readonly ILoggerFactory _loggerFactory;

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
			services.AddLogging();

			// Configuration
			services.AddOptions();
			services.Configure<SmtpSettings>(_configuration.GetSection("Smtp"));

			// Roadkill IoC
			string connectionString = _configuration["ConnectionString"];
			CoreDependencyInjection.ConfigureServices(services, connectionString, _loggerFactory.CreateLogger("Startup"));
			ApiDependencyInjection.ConfigureServices(services);

			// Authentication (ASP.NET Identity)
			ApiDependencyInjection.ConfigureIdentity(services);

			// Authorization (JWT)
			ApiDependencyInjection.ConfigureJwt(services);

			services.AddMvc();
			services.AddSwaggerDocument();

			var provider = services.BuildServiceProvider();
			return provider;
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			app.UseSwagger();
			app.UseSwaggerUi3();
			app.UseStaticFiles();
			app.UseAuthentication();
		    app.UseProblemDetails();
			app.UseMvc();
		}
	}
}
