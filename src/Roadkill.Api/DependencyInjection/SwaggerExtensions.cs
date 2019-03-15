using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.SwaggerGeneration.Processors.Security;

namespace Roadkill.Api
{
	public static class SwaggerExtensions
	{
		public static IServiceCollection AddAllSwagger(this IServiceCollection services)
		{
			services.AddSwaggerDocument(document =>
			{
				// Add an authenticate button to Swagger for JWT tokens
				document.OperationProcessors.Add(new OperationSecurityScopeProcessor("JWT"));
				var swaggerSecurityScheme = new SwaggerSecurityScheme
				{
					Type = SwaggerSecuritySchemeType.ApiKey,
					Name = "Authorization",
					In = SwaggerSecurityApiKeyLocation.Header,
					Description = "Type into the textbox: Bearer {your JWT token}. You can get a JWT token from /Authorization/Authenticate."
				};

				document.ApiGroupNames = new[] { "3" };
				document.DocumentName = "v3";
				document.DocumentProcessors.Add(new SecurityDefinitionAppender("JWT", swaggerSecurityScheme));
				document.PostProcess = d => d.Info.Title = "Roadkill v3 API";
			});

			return services;
		}
	}
}
