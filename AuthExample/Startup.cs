using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(AuthExample.Startup))]
namespace AuthExample
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "local.settings.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var hostEnvironment = (IHostEnvironment)builder.Services
                .Where(s => s.ServiceType == typeof(IHostEnvironment))
                .First().ImplementationInstance;

            var configuration = builder.GetContext().Configuration;

            builder.Services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            builder.Services
                .AddSingleton<TokenCredential>(new DefaultAzureCredential(
                    includeInteractiveCredentials: hostEnvironment.IsDevelopment()))
                .AddSingleton<SignatureHelper>()
                .AddSingleton<CustomValidator>()
                .AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

            builder.Services
                .AddAuthentication("Test")
                .AddScheme<JwtBearerOptions, ExampleHandler>("Test", options => _configure(options, configuration));
        }

        private static void _configure(JwtBearerOptions options, IConfiguration configuration)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                RequireAudience = false,
                RequireExpirationTime = true,

                RequireSignedTokens = false,

                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true
            };
        }

        private class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
        {
            private readonly CustomValidator _validator;

            public ConfigureJwtBearerOptions(CustomValidator validator)
            {
                _validator = validator;
            }

            public void PostConfigure(string name, JwtBearerOptions options)
            {
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(_validator);
            }
        }
    }
}
