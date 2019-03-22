using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roadkill.Api;
using Roadkill.Api.JWT;
using Roadkill.Core.Authorization;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Api.Controllers
{
	public class IntegrationTestsWebFactory : WebApplicationFactory<Startup>
	{
		private static Dictionary<string, string> _testConfigValues = new Dictionary<string, string>
		{
			{ "Postgres:ConnectionString", "host=localhost;port=5432;database=roadkilltests;username=roadkill;password=roadkill;" },
			{ "Smtp:Host", "smtp.gmail.com" },
			{ "Smtp:Port", "587" },
			{ "Smtp:UseSsl", "true" },
			{ "Smtp:Username", "bob" },
			{ "Smtp:Password", "mypassword" },
			{ "Jwt:Password", "12345678901234567890" },
			{ "Jwt:ExpiresDays", "7" }
		};

		/// <summary>
		/// Gets or sets the ITestOutputHelper which enables logging to the XUnit console
		/// via an MartinCostello.Logging.XUnit and an ILogger.
		/// </summary>
		public ITestOutputHelper TestOutputHelper { get; set; }

		public RoadkillUser AdminUser { get; set; }

		public string AdminUserPassword { get; set; }

		public RoadkillUser EditorUser { get; set; }

		public string EditorUserPassword { get; set; }

		protected override TestServer CreateServer(IWebHostBuilder builder)
		{
			var server = base.CreateServer(builder);
			var provider = server.Host.Services;

			// Clean the postgres database
			var documentStore = provider.GetService<IDocumentStore>();
			documentStore.Advanced.Clean.DeleteAllDocuments();

			// Create two users
			var manager = provider.GetService<UserManager<RoadkillUser>>();
			CreateAdminUser(manager);
			CreateEditorUser(manager);

			return server;
		}

		protected override IWebHostBuilder CreateWebHostBuilder()
		{
			var configBuilder = new ConfigurationBuilder();
			configBuilder.AddInMemoryCollection(_testConfigValues);
			var config = configBuilder.Build();

			var builder = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseConfiguration(config)
				.ConfigureLogging(loggingBuilder => loggingBuilder.AddXUnit(TestOutputHelper))
				.UseStartup<Startup>();

			return builder;
		}

		private void CreateAdminUser(UserManager<RoadkillUser> manager)
		{
			AdminUser = new RoadkillUser()
			{
				UserName = "admin@example.org",
				Email = "admin@example.org",
				EmailConfirmed = true
			};
			AdminUserPassword = "Password1234567890";

			var result = manager.CreateAsync(AdminUser, AdminUserPassword).GetAwaiter().GetResult();
			if (!result.Succeeded)
			{
				string errors = string.Join("\n", result.Errors.ToList().Select(x => x.Description));
				throw new Exception("Failed to create admin user - " + errors);
			}

			manager.AddClaimAsync(AdminUser, new Claim(ClaimTypes.Role, RoleNames.Admin)).GetAwaiter()
				.GetResult();
		}

		private void CreateEditorUser(UserManager<RoadkillUser> manager)
		{
			EditorUser = new RoadkillUser()
			{
				UserName = "editor@example.org",
				Email = "editor@example.org",
				EmailConfirmed = true
			};
			EditorUserPassword = "Password1234567890";

			var result = manager.CreateAsync(EditorUser, EditorUserPassword).GetAwaiter().GetResult();
			if (!result.Succeeded)
			{
				string errors = string.Join("\n", result.Errors.ToList().Select(x => x.Description));
				throw new Exception("Failed to create editor user - " + errors);
			}

			manager.AddClaimAsync(AdminUser, new Claim(ClaimTypes.Role, RoleNames.Editor)).GetAwaiter()
				.GetResult();
		}
	}
}
