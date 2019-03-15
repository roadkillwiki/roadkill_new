using System;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace Roadkill.Api.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseJsonHealthChecks(this IApplicationBuilder app)
		{
			var options = new HealthCheckOptions
			{
				ResponseWriter = async (context, report) =>
				{
					var statusObject = new
					{
						Status = report.Status.ToString(),
						Errors = report.Entries.Select(error => new
						{
							HealthCheckName = error.Key,
							Description = error.Value.Description
						})
					};

					var json = JsonConvert.SerializeObject(statusObject);
					context.Response.ContentType = MediaTypeNames.Application.Json;
					await context.Response.WriteAsync(json);
				}
			};

			app.UseHealthChecks("/healthcheck", options);

			return app;
		}
	}
}
