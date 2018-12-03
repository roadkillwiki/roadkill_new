using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roadkill.Core.Configuration;

using ApiDependencyInjection = Roadkill.Api.DependencyInjection;
using CoreDependencyInjection = Roadkill.Core.DependencyInjection;

namespace Roadkill.Api
{
	public class Startup
	{
		public IConfigurationRoot Configuration { get; set; }

		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder();
			builder
				.SetBasePath(Path.Combine(env.ContentRootPath))
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
				.AddEnvironmentVariables();

			Configuration = builder.Build();
		}

		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.AddLogging();

			// Configuration
			services.AddOptions();
			services.Configure<SmtpSettings>(Configuration.GetSection("Smtp"));
			
			// Roadkill IoC
			string connectionString = Configuration["ConnectionString"];
			CoreDependencyInjection.ConfigureServices(services, connectionString);
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
			app.UseExceptionHandler("/error");
			app.UseSwagger();
			app.UseSwaggerUi3();
			app.UseStaticFiles();
			app.UseAuthentication();
			app.UseMvc();
		}
	}
}