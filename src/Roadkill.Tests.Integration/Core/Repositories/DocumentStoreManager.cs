using System;
using System.Collections.Concurrent;
using System.IO;
using Marten;
using Roadkill.Core.Entities;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Core.Repositories
{
	public static class DocumentStoreManager
	{
		private static readonly ConcurrentDictionary<string, IDocumentStore> _documentStores = new ConcurrentDictionary<string, IDocumentStore>();

		public static string ConnectionString = "host=localhost;port=5432;database=roadkilltests;username=roadkill;password=roadkill;";

		private static void UpdateConnectionForGoogleCloudBuild()
		{
			if (Directory.GetCurrentDirectory().Contains("/workspace/"))
			{
				ConnectionString = "host=roadkill-postgres;port=5432;database=roadkilltests;username=roadkill;password=roadkill;";
			}
		}

		public static IDocumentStore GetMartenDocumentStore(Type testClassType, ITestOutputHelper outputHelper)
		{
			UpdateConnectionForGoogleCloudBuild();

			string documentStoreSchemaName = "";

			// Use a different schema for each test class, so their data is isolated
			if (testClassType != null)
			{
				documentStoreSchemaName = testClassType.Name;
			}

			if (_documentStores.ContainsKey(documentStoreSchemaName))
			{
				outputHelper.WriteLine("DocumentStoreManager: found doc store in cache {0}", documentStoreSchemaName);
				return _documentStores[documentStoreSchemaName];
			}

			IDocumentStore docStore = CreateDocumentStore(ConnectionString, documentStoreSchemaName, outputHelper);
			_documentStores.AddOrUpdate(documentStoreSchemaName, docStore, (s, store) => store);
			outputHelper.WriteLine($"DocumentStoreManager: created doc store {documentStoreSchemaName} using {ConnectionString}");

			return _documentStores[documentStoreSchemaName];
		}

		private static IDocumentStore CreateDocumentStore(string connectionString, string schemaName, ITestOutputHelper outputHelper)
		{
			try
			{
				StoreOptions options = ConfigureOptions(connectionString, schemaName);
				var documentStore = new DocumentStore(options);

				return documentStore;
			}
			catch (Exception e)
			{
				outputHelper.WriteLine("CreateDocumentStore failed:");
				outputHelper.WriteLine(e.ToString());
				return null;
			}
		}

		private static StoreOptions ConfigureOptions(string connectionString, string schemaName)
		{
			var options = new StoreOptions();
			options.AutoCreateSchemaObjects = AutoCreate.All;
			options.DatabaseSchemaName = schemaName;
			options.Connection(connectionString);
			options.Schema.For<User>().Index(x => x.Id);
			options.Schema.For<Page>().Identity(x => x.Id);
			options.Schema.For<Page>().Index(x => x.Id);
			options.Schema.For<PageVersion>().Index(x => x.Id);

			return options;
		}
	}
}
