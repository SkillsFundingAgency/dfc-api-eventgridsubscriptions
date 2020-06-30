using DFC.EventGridSubscriptions.Data.Models;
using DFC.EventGridSubscriptions.Services.Interface;
using DFC.EventGridSubscriptions.Services.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace DFC.EventGridSubscriptions.Services.Sources
{
    public class KeyVaultSource : IConfigurationSource
    {
        private readonly List<string> keyVaultKeys;
        private readonly IKeyVaultService keyVaultService;

        public KeyVaultSource(List<string> keyVaultKeys, IKeyVaultService keyVaultService)
        {
            this.keyVaultKeys = keyVaultKeys;
            this.keyVaultService = keyVaultService;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var config = new KeyVaultConfigurationProvider(keyVaultKeys, keyVaultService);
            return config;
        }
    }
}
