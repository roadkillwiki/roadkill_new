using Marten;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Models;
using Scrutor;
using System;

namespace Roadkill.Core
{
	public class DependencyInjection
	{
		public static void ConfigureServices(IServiceCollection services, string postgresConnectionString)
		{
			// Postgres + Marten
			DocumentStore documentStore = CreateDocumentStore(postgresConnectionString);
			services.AddSingleton<IDocumentStore>(documentStore);

			// ElasticSearch TODO

			services.Scan(scan => scan
			   .FromAssemblyOf<Roadkill.Core.DependencyInjection>()

			   // SomeClass => ISomeClass
			   .AddClasses()
			   .UsingRegistrationStrategy(RegistrationStrategy.Skip)
			   .AsMatchingInterface()
			   .WithTransientLifetime()
		   );
		}

		internal static DocumentStore CreateDocumentStore(string connectionString)
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
						.OnDatabaseCreated(_ =>
						{
							Console.WriteLine("Postgres 'roadkill' database created");
						});
				});

				options.Connection(connectionString);
				options.Schema.For<User>().Index(x => x.Id);
				options.Schema.For<Page>().Identity(x => x.Id);
				options.Schema.For<Page>().Index(x => x.Id);
				options.Schema.For<PageVersion>().Index(x => x.Id);
			});

			return documentStore;
		}
	}
}