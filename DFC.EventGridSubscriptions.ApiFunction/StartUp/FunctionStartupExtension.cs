using DFC.Compui.Cosmos;
using DFC.Compui.Cosmos.Contracts;
using DFC.EventGridSubscriptions.ApiFunction.StartUp;
using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services;
using DFC.EventGridSubscriptions.Services.Extensions;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

[assembly: FunctionsStartup(typeof(FunctionStartupExtension))]

namespace DFC.EventGridSubscriptions.ApiFunction.StartUp
{
    /// <summary>
    /// The function startup extension.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FunctionStartupExtension : FunctionsStartup
    {
        /// <inheritdoc/>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(GetCustomSettingsPath())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();

            builder.Services.AddOptions<EventGridSubscriptionClientOptions>()
             .Configure<IConfiguration>((settings, configuration) => { configuration.GetSection("EventGridSubscriptionClientOptions").Bind(settings); });

            builder.Services.AddOptions<AdvancedFilterOptions>()
               .Configure<IConfiguration>((settings, configuration) => { configuration.GetSection("AdvancedFilterOptions").Bind(settings); });

            builder.Services.AddKeyVaultClient($"https://{config["keyvault_name"]}.vault.azure.net");
            config = configBuilder.AddKeyVaultConfigurationProvider(config.GetSection("KeyVaultOptions:ApplicationKeyVaultKeys").Get<List<string>>(), builder.Services.BuildServiceProvider()).Build();

            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddTransient<ISubscriptionService, SubscriptionService>();
            builder.Services.AddEventGridManagementClient();

            var cosmosDbConnectionEventGridSubscriptions = config.GetSection("Configuration:CosmosDbConnections:EventGridSubscriptions").Get<CosmosDbConnection>();
            builder.Services.AddDocumentServices<SubscriptionModel>(cosmosDbConnectionEventGridSubscriptions, Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToUpperInvariant() == "DEVELOPMENT");
        }

        private static string GetCustomSettingsPath()
        {
            var home = Environment.GetEnvironmentVariable("HOME");
            string? path;
            if (home != null)
            {
                // We're on Azure
                path = Path.Combine(home, "site", "wwwroot");
            }
            else
            {
                // Running locally
                path = new Uri(Assembly.GetExecutingAssembly().CodeBase!).LocalPath;

                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetDirectoryName(path);
                    DirectoryInfo parentDir = Directory.GetParent(path);
                    path = parentDir.FullName;
                }
            }

            return path ?? string.Empty;
        }
    }
}
