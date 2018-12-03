using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Marten;
using Roadkill.Core.Models;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Repositories
{
	public class DocumentStoreManager
	{
		private static readonly ConcurrentDictionary<string, IDocumentStore> _documentStores = new ConcurrentDictionary<string, IDocumentStore>();
		public static string ConnectionString => "host=localhost;port=5432;database=roadkill;username=postgres;password=;";

		public static IDocumentStore GetMartenDocumentStore(Type testClassType, ITestOutputHelper outputHelper)
		{
			string documentStoreSchemaName = "";

			// Use a different schema for each test class, so their data is isolated
			if (testClassType != null)
				documentStoreSchemaName = testClassType.Name;

			if (_documentStores.ContainsKey(documentStoreSchemaName))
			{
				outputHelper.WriteLine("GetMartenDocumentStore: found doc store in cache {0}", documentStoreSchemaName);
				return _documentStores[documentStoreSchemaName];
			}

			IDocumentStore docStore = CreateDocumentStore(ConnectionString, documentStoreSchemaName, outputHelper);
			_documentStores.AddOrUpdate(documentStoreSchemaName, docStore, (s, store) => store);

			return _documentStores[documentStoreSchemaName];
		}

		internal static IDocumentStore CreateDocumentStore(string connectionString, string schemaName, ITestOutputHelper outputHelper)
		{
			try
			{
				StoreOptions options = ConfigureOptions(connectionString, schemaName, outputHelper);
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

		private static StoreOptions ConfigureOptions(string connectionString, string schemaName, ITestOutputHelper outputHelper)
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