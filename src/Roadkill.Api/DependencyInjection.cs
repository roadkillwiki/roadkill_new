using System.Security.Claims;
using System.Text;
using MailKit;
using MailKit.Net.Smtp;
using Marten.AspNetIdentity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Roadkill.Text;
using Roadkill.Text.Sanitizer;
using Roadkill.Text.TextMiddleware;
using Scrutor;

namespace Roadkill.Api
{
    public class DependencyInjection
    {
        public const string JwtPassword = "password123456789"; // TODO: make configurable

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            // MailKit
            services.AddScoped<IMailTransport, SmtpClient>();

            // Markdown
            services.AddScoped<TextSettings>();
            services.AddScoped<IHtmlWhiteListProvider, HtmlWhiteListProvider>();
            services.AddScoped<ITextMiddlewareBuilder>(provider =>
            {
                var textSettings = provider.GetService<TextSettings>();
                var logger = provider.GetService<ILogger>();

                return TextMiddlewareBuilder.Default(textSettings, logger);
            });

            services.Scan(scan => scan
                .FromAssemblyOf<Roadkill.Api.DependencyInjection>()

                // SomeClass => ISomeClass
                .AddClasses()
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsMatchingInterface()
                .WithTransientLifetime()
            );
        }

        public static void ConfigureJwt(IServiceCollection services)
        {
            var key = Encoding.ASCII.GetBytes(JwtPassword);
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
            services.AddAuthorization(ConfigureJwtClaimsPolicies);
        }

        private static void ConfigureJwtClaimsPolicies(AuthorizationOptions options)
        {
            options.AddPolicy("Admins", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
            options.AddPolicy("Editors", policy => policy.RequireClaim(ClaimTypes.Role, "Editor"));
        }

        public static void ConfigureIdentity(IServiceCollection services)
        {
            services.AddIdentity<RoadkillUser, IdentityRole>(
                    options =>
                    {
                        options.Password.RequireDigit = false;
                        options.Password.RequiredUniqueChars = 0;
                        options.Password.RequireLowercase = false;
                        options.Password.RequiredLength = 0;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = false;
                    })
                .AddMartenStores<RoadkillUser, IdentityRole>()
                .AddDefaultTokenProviders();
        }
    }
}