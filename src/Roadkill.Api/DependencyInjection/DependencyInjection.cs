using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MailKit;
using MailKit.Net.Smtp;
using Marten.AspNetIdentity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Roadkill.Api.JWT;
using Roadkill.Api.Settings;
using Roadkill.Core.Authorization;
using Roadkill.Text;
using Roadkill.Text.Sanitizer;
using Roadkill.Text.TextMiddleware;
using Scrutor;
using ApiDependencyInjection = Roadkill.Api.DependencyInjection.DependencyInjection;

namespace Roadkill.Api.DependencyInjection
{
	[SuppressMessage("Stylecop", "CA1052", Justification = "Can't be static, it needs a type for scanning")]
	[SuppressMessage("Stylecop", "CA1724", Justification = "The name DependencyInjection is fine to share")]
    public class DependencyInjection
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            // MailKit
            services.AddScoped<IMailTransport, SmtpClient>();

            // Markdown
            services.AddScoped<TextSettings>();
            services.AddScoped<IHtmlWhiteListProvider, HtmlWhiteListProvider>();
            services.AddScoped<ITextMiddlewareBuilder>(TextMiddlewareBuilder.Default);

            // JWT
            services.AddScoped<IJwtTokenProvider>(provider =>
            {
	            var settings = provider.GetService<JwtSettings>();
	            var tokenHandler = new JwtSecurityTokenHandler();
	            return new JwtTokenProvider(settings, tokenHandler);
            });

	        // SomeClass => ISomeClass
            services.Scan(scan => scan
                .FromAssemblyOf<ApiDependencyInjection>()
                .AddClasses()
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsMatchingInterface()
                .WithTransientLifetime());
        }

        public static void ConfigureJwt(IServiceCollection services, JwtSettings jwtSettings)
        {
            var password = Encoding.ASCII.GetBytes(jwtSettings.Password);
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
                        IssuerSigningKey = new SymmetricSecurityKey(password),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
            services.AddAuthorization(ConfigureJwtClaimsPolicies);
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

        private static void ConfigureJwtClaimsPolicies(AuthorizationOptions options)
        {
            options.AddPolicy(PolicyNames.Admin, policy => policy.RequireClaim(ClaimTypes.Role, RoleNames.Admin));
            options.AddPolicy(PolicyNames.Editor, policy => policy.RequireClaim(ClaimTypes.Role, RoleNames.Editor));
        }
    }
}
