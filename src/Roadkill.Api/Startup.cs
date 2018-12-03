using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Marten.AspNetIdentity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSwag.AspNetCore;
using Roadkill.Core.Configuration;

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

			string connectionString = Configuration["ConnectionString"];
			Roadkill.Core.DependencyInjection.ConfigureServices(services, connectionString);
			Roadkill.Api.DependencyInjection.ConfigureServices(services);

			services.AddIdentity<RoadkillUser, IdentityRole>(options =>
			{
				options.Password.RequireDigit = false;
				options.Password.RequiredUniqueChars = 0;
				options.Password.RequireLowercase = false;
				options.Password.RequiredLength = 0;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
			})
			.AddMartenStores<RoadkillUser, IdentityRole>()
			.AddDefaultTokenProviders();

			services.AddAuthorization(options =>
			{
				options.AddPolicy("ApiUser", policy => policy.RequireClaim("ApiUser", "CanAddPage"));
			});

			services.AddOptions();
			services.Configure<SmtpSettings>(Configuration.GetSection("Smtp"));
			services.AddSwaggerDocument();
			services.AddMvc();

			var provider = services.BuildServiceProvider();
			return provider;
		}

		public async Task Add(UserManager<RoadkillUser> userManager)
		{
			var newUser = new RoadkillUser()
			{
				UserName = "chris@example.org",
				Email = "chris@example.org",
			};

			await userManager.CreateAsync(newUser, "password");
			await userManager.AddClaimAsync(newUser, new Claim("ApiUser", "CanAddPage"));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			app.UseExceptionHandler("/error");
			app.UseSwaggerUi3();
			app.UseStaticFiles();
			app.UseAuthentication();
			app.UseMvc();
		}
	}

	public class RoadkillUser : IdentityUser, IClaimsUser
	{
		public IList<byte[]> Claims { get; set; }
	}
}