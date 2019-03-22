using System;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roadkill.Core.Entities;

namespace Roadkill.Core.Extensions
{
	public static class MartenServiceCollectionExtensions
	{
		public static IServiceCollection AddMartenDocumentStore(this IServiceCollection services, string connectionString, ILogger logger, bool throwOnError = false)
		{
			try
			{
				var documentStore = CreateDocumentStore(connectionString, logger);

				if (documentStore != null)
				{
					services.AddSingleton<IDocumentStore>(documentStore);
				}

				return services;
			}
			catch (Exception ex)
			{
				if (throwOnError)
				{
					throw;
				}

				logger.LogError(ex, "A Postgres/Marten related error occurred. Check your database connection strings are correct (see exception for more information).");
				return null;
			}
		}

		private static DocumentStore CreateDocumentStore(string connectionString, ILogger logger)
		{
				var documentStore = DocumentStore.For(options =>
				{
					options.CreateDatabasesForTenants(c =>
					{
						c.ForTenant()
							.CheckAgainstPgDatabase()
							.WithOwner("roadkill")
							.WithEncoding("UTF-8")
							.ConnectionLimit(-1)
							.OnDatabaseCreated(connection =>
							{
								logger.LogInformation($"Postgres database created by Marten.");
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
