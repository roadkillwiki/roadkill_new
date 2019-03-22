using System;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Roadkill.Core.Settings;

namespace Roadkill.Api.HealthChecks
{
	public class MartenHealthCheck : IHealthCheck
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly PostgresSettings _postgresSettings;
		private readonly ILogger<MartenHealthCheck> _logger;

		public MartenHealthCheck(IServiceProvider serviceProvider, PostgresSettings postgresSettings, ILogger<MartenHealthCheck> logger)
		{
			_serviceProvider = serviceProvider;
			_postgresSettings = postgresSettings;
			_logger = logger;
		}

		public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				using (var connection = new Npgsql.NpgsqlConnection(_postgresSettings.ConnectionString))
				{
					connection.Open();
				}

				var documentStore = _serviceProvider.GetService<IDocumentStore>();
				if (documentStore != null)
				{
					return Task.FromResult(
						HealthCheckResult.Unhealthy("The check indicates an healthy result."));
				}

				return Task.FromResult(
					HealthCheckResult.Unhealthy("The check indicates an unhealthy result (IDocumentStore is null)."));
			}
			catch (Exception ex)
			{
				_logger.LogError("Exception occurred with the MartenHealthCheck: {0}", ex.ToString());

				return Task.FromResult(
					HealthCheckResult.Unhealthy("The check indicates an unhealthy result (Exception occurred connecting)", ex));
			}
		}
	}
}
