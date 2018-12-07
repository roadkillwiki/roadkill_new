using System;
using System.Diagnostics.CodeAnalysis;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roadkill.Core.Entities;
using Scrutor;

namespace Roadkill.Core
{
	[SuppressMessage("Stylecop", "CA1052", Justification = "Can't be static, it needs a type for scanning")]
	[SuppressMessage("Stylecop", "CA1724", Justification = "The name DependencyInjection is fine to share")]
	public class DependencyInjection
	{
		public static void ConfigureServices(
			IServiceCollection services,
			string postgresConnectionString,
			ILogger logger)
		{
			// Postgres + Marten
			var documentStore = CreateDocumentStore(postgresConnectionString, logger);

			if (documentStore != null)
			{
				services.AddSingleton<IDocumentStore>(documentStore);
			}

			// ElasticSearch TODO

			// Configure default conventions
			// SomeClass => ISomeClass
			services.Scan(scan => scan
				.FromAssemblyOf<DependencyInjection>()
				.AddClasses()
				.UsingRegistrationStrategy(RegistrationStrategy.Skip)
				.AsMatchingInterface()
				.WithTransientLifetime());
		}

		private static DocumentStore CreateDocumentStore(string connectionString, ILogger logger)
		{
			try
			{
				var documentStore = DocumentStore.For(options =>
				{
					options.CreateDatabasesForTenants(c =>
					{
						c.MaintenanceDatabase(connectionString);
						c.ForTenant()
							.CheckAgainstPgDatabase()
							.WithOwner("roadkill")
							.WithEncoding("UTF-8")
							.ConnectionLimit(-1)
							.OnDatabaseCreated(_ => { logger.LogInformation("Postgres 'roadkill' database created"); });
					});

					options.Connection(connectionString);
					options.Schema.For<User>().Index(x => x.Id);
					options.Schema.For<Page>().Identity(x => x.Id);
					options.Schema.For<Page>().Index(x => x.Id);
					options.Schema.For<PageVersion>().Index(x => x.Id);
				});

				return documentStore;
			}
			catch (Exception exception)
			{
				logger.LogError(exception, "A Postgres/Marten related error occurred. Check your database connection strings are correct (see exception for more information).");
				return null;
			}
		}
	}
}
