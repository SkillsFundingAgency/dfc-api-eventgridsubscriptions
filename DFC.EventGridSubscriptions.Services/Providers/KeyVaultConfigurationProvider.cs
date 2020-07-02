using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DFC.EventGridSubscriptions.Services.Providers
{
    public class KeyVaultConfigurationProvider : ConfigurationProvider
    {
        private readonly List<string> keyVaultKeys;
        private readonly IKeyVaultService keyVaultService;

        public KeyVaultConfigurationProvider(List<string> keyVaultKeys, IKeyVaultService keyVaultService)
        {
            this.keyVaultKeys = keyVaultKeys ?? throw new ArgumentNullException(nameof(keyVaultKeys));
            this.keyVaultService = keyVaultService;
        }

        public override void Load()
        {   
            if (keyVaultKeys.Any())
            {
                foreach (var keyVaultKey in keyVaultKeys)
                {
                    var keyVaultSecret = keyVaultService.GetSecretAsync(keyVaultKey).GetAwaiter().GetResult();
                    Data.Add(keyVaultKey, keyVaultSecret);
                }
            }
        }
    }
}
