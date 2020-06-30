using DFC.EventGridSubscriptions.ApiFunction.StartUp;
using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Data.Models;
using DFC.EventGridSubscriptions.Services;
using DFC.EventGridSubscriptions.Services.Extensions;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();

            builder.Services.AddOptions<EventGridSubscriptionClientOptions>()
             .Configure<IConfiguration>((settings, configuration) => { configuration.GetSection("EventGridSubscriptionClientOptions").Bind(settings); });

            builder.Services.AddOptions<AdvancedFilterOptions>()
               .Configure<IConfiguration>((settings, configuration) => { configuration.GetSection("AdvancedFilterOptions").Bind(settings); });

            config = configBuilder.AddKeyVaultConfigurationProvider(config.GetSection("KeyVaultOptions:ApplicationKeyVaultKeys").Get<List<string>>(), builder.Services.BuildServiceProvider()).Build();

            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddTransient<ISubscriptionRegistrationService, SubscriptionRegistrationService>();
            builder.Services.AddEventGridManagementClient();
        }
    }
}
