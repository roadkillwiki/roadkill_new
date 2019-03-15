using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Roadkill.Api.Middleware;

namespace Roadkill.Api.Extensions
{
	public static class StartupExtensions
	{
		public static IApplicationBuilder UseJsonExceptionHandler(this IApplicationBuilder app, IHostingEnvironment environment)
		{
			app.UseExceptionHandler(new ExceptionHandlerOptions()
			{
				ExceptionHandler = new JsonExceptionMiddleware(environment).Invoke
			});

			return app;
		}
	}
}
