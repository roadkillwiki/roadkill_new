using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roadkill.Core.Configuration;
using Roadkill.Core.Entities;
using Roadkill.Core.Settings;
using Scrutor;

namespace Roadkill.Core
{
	[SuppressMessage("Stylecop", "CA1052", Justification = "Can't be static, it needs a type for scanning")]
	[SuppressMessage("Stylecop", "CA1724", Justification = "The name DependencyInjection is fine to share")]
	public class DependencyInjection
	{
		public static void ConfigureServices(
			IServiceCollection services,
			IConfigurationRoot configuration,
			ILogger logger)
		{
			// Settings
			var smtpSettings = new SmtpSettings();
			var postgresSettings = new PostgresSettings();
			configuration.Bind("Smtp", smtpSettings);
			configuration.Bind("Postgres", postgresSettings);

			GuardAllConfigProperties("Smtp", smtpSettings);
			GuardAllConfigProperties("Postgres", smtpSettings);

			services.AddSingleton(smtpSettings);
			services.AddSingleton(postgresSettings);

			// Postgres + Marten
			var documentStore = CreateDocumentStore(postgresSettings.ConnectionString, logger);

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

		public static void GuardAllConfigProperties<T>(string sectionName, T instance)
		{
			IEnumerable<PropertyInfo> publicProperties = typeof(T).GetProperties().Where(x => x.MemberType == MemberTypes.Property);
			foreach (PropertyInfo property in publicProperties)
			{
				string value = Convert.ToString(property.GetValue(instance), CultureInfo.InvariantCulture);
				if (string.IsNullOrEmpty(value))
				{
					throw new InvalidOperationException($"Setting: {sectionName}__{property.Name} is missing or empty");
				}
			}
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
