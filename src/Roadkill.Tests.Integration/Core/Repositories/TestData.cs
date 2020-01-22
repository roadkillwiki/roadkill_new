using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Marten;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Roadkill.Api;
using Roadkill.Api.Authorization.JWT;
using Roadkill.Core.Entities;
using Roadkill.Core.Entities.Authorization;
using Roadkill.Core.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Core.Repositories
{
	/// <summary>
	/// Creates an editor, admin and homepage in the "roadkill" postgres database.
	/// </summary>
	public class TestData
	{
		private readonly ITestOutputHelper _outputHelper;
		private readonly Fixture _fixture;
		private IDocumentStore _documentStore;

		public TestData(ITestOutputHelper outputHelperHelper)
		{
			_outputHelper = outputHelperHelper;
			_fixture = new Fixture();
		}


		public static Dictionary<string, string> TestConfigValues = new Dictionary<string, string>
		{
			{ "Postgres:ConnectionString", "host=localhost;port=5432;database=roadkill;username=roadkill;password=roadkill;" },
			{ "Smtp:Host", "smtp.gmail.com" },
			{ "Smtp:Port", "587" },
			{ "Smtp:UseSsl", "true" },
			{ "Smtp:Username", "bob" },
			{ "Smtp:Password", "mypassword" },
			{ "Jwt:Password", "12345678901234567890" },
			{ "Jwt:ExpiresDays", "7" }
		};

		[Fact(Skip = "This is just a data setup test.")]
		public async Task CreateUsersAndHomepage()
		{
			await CreateUsers();
			await CreateHomepage();
		}

		private async Task CreateUsers()
		{
			// Create users via the UserManager as it needs ASP.NET Identity roles etc.
			var configBuilder = new ConfigurationBuilder();
			configBuilder.AddInMemoryCollection(TestConfigValues);
			var configuration = configBuilder.Build();

			var startup = new Startup(null, new NullLogger<Startup>(), configuration);

			var services = new ServiceCollection();
			startup.ConfigureServices(services);
			var provider = services.BuildServiceProvider();

			var manager = provider.GetService<UserManager<RoadkillIdentityUser>>();
			_documentStore = provider.GetService<IDocumentStore>();
			_documentStore.Advanced.Clean.CompletelyRemoveAll();

			await CreateAdminUser(manager);
			await CreateEditorUser(manager);
		}

		private async Task CreateHomepage()
		{
			// Create a homepage with text through repositories as it's simpler
			string createdBy = "editor";
			DateTime createdOn = DateTime.Today;

			PageRepository repository = CreatePageRepository();

			Page page = _fixture.Create<Page>();
			page.Id = -1; // should be reset
			page.CreatedBy = createdBy;
			page.CreatedOn = createdOn;
			page.LastModifiedBy = createdBy;
			page.LastModifiedOn = createdOn;
			page.Tags = "homepage";

			Page newPage = await repository.AddNewPageAsync(page);
			_outputHelper.WriteLine($"Created homepage - id: {page.Id}");

			PageVersionRepository pageRepository = CreatePageVersionRepository();
			PageVersion pageVersion = await pageRepository.AddNewVersionAsync(newPage.Id, "## This is some markdown\nAnd some **bold** text", "editor");
			_outputHelper.WriteLine($"Created homepage version - id: {pageVersion.Id}");
		}

		private PageVersionRepository CreatePageVersionRepository()
		{
			return new PageVersionRepository(_documentStore);
		}

		private PageRepository CreatePageRepository()
		{
			return new PageRepository(_documentStore);
		}

		private async Task  CreateAdminUser(UserManager<RoadkillIdentityUser> manager)
		{
			var adminUser = new RoadkillIdentityUser()
			{
				UserName = "admin@example.org",
				Email = "admin@example.org",
				EmailConfirmed = true
			};
			string password = "Password1234567890";

			var result = manager.CreateAsync(adminUser, password).GetAwaiter().GetResult();
			if (!result.Succeeded)
			{
				string errors = string.Join("\n", result.Errors.ToList().Select(x => x.Description));
				throw new Exception("Failed to create admin user - " + errors);
			}

			manager.AddClaimAsync(adminUser, RoadkillClaims.AdminClaim).GetAwaiter()
				.GetResult();

			_outputHelper.WriteLine("Created admin user with role");
		}

		private async Task CreateEditorUser(UserManager<RoadkillIdentityUser> manager)
		{
			var user = new RoadkillIdentityUser()
			{
				UserName = "editor@example.org",
				Email = "editor@example.org",
				EmailConfirmed = true
			};
			string password = "Password1234567890";

			var result = manager.CreateAsync(user, password).GetAwaiter().GetResult();
			if (!result.Succeeded)
			{
				string errors = string.Join("\n", result.Errors.ToList().Select(x => x.Description));
				throw new Exception("Failed to create editor user - " + errors);
			}

			manager.AddClaimAsync(user, RoadkillClaims.EditorClaim).GetAwaiter()
				.GetResult();

			_outputHelper.WriteLine("Created editor user with role");
		}
	}
}
