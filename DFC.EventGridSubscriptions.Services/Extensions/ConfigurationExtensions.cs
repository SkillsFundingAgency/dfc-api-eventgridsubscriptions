using DFC.EventGridSubscriptions.Data.Models;
using DFC.EventGridSubscriptions.Services.Interface;
using DFC.EventGridSubscriptions.Services.Sources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace DFC.EventGridSubscriptions.Services.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddKeyVaultConfigurationProvider(
            this IConfigurationBuilder configuration, List<string> keyVaultKeys, ServiceProvider serviceProvider)
        {
            configuration.Add(new KeyVaultSource(keyVaultKeys, serviceProvider.GetRequiredService<IKeyVaultService>()));
            return configuration;
        }
    }
}
