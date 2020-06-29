using DFC.EventGridSubscriptions.ApiFunction.StartUp;
using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);

            builder.Services.AddTransient<ISubscriptionRegistrationService, SubscriptionRegistrationService>();
            builder.Services.AddEventGridManagementClient();

            builder.Services.AddOptions<EventGridSubscriptionClientOptions>()
                .Configure<IConfiguration>((settings, configuration) => { configuration.GetSection("EventGridSubscriptionClientOptions").Bind(settings); });

            builder.Services.AddOptions<AdvancedFilterOptions>()
               .Configure<IConfiguration>((settings, configuration) => { configuration.GetSection("AdvancedFilterOptions").Bind(settings); });
        }
    }
}
