using System;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roadkill.Core.Entities;

namespace Roadkill.Core.Extensions
{
	public static class MartenServiceCollectionExtensions
	{
		public static IServiceCollection AddMartenDocumentStore(this IServiceCollection services, string connectionString, ILogger logger)
		{
			var documentStore = CreateDocumentStore(connectionString, logger);

			if (documentStore != null)
			{
				services.AddSingleton<IDocumentStore>(documentStore);
			}

			return services;
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
