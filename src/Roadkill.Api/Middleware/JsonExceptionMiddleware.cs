using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Roadkill.Api.Middleware
{
	public class JsonExceptionMiddleware
	{
		private readonly IHostingEnvironment _environment;

		public JsonExceptionMiddleware(IHostingEnvironment environment)
		{
			_environment = environment;
		}

		public async Task Invoke(HttpContext context)
		{
			// From https://recaffeinate.co/post/serialize-errors-as-json-in-aspnetcore/
			context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

			var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
			if (ex == null)
			{
				return;
			}

			var error = new ErrorDetails();

			if (_environment.IsDevelopment())
			{
				error.Title = ex.Message;
				error.Detail = ex.StackTrace;
			}
			else
			{
				error.Title = "A server error ocurred.";
				error.Detail = ex.Message;
			}

			context.Response.ContentType = "application/json";

			using (var writer = new StreamWriter(context.Response.Body))
			{
				new JsonSerializer().Serialize(writer, error);
				await writer.FlushAsync().ConfigureAwait(false);
			}
		}

		private class ErrorDetails
		{
			public string Title { get; set; }

			public string Detail { get; set; }
		}
	}
}
