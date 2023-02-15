using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DFC.EventGridSubscriptions.Services.Interface;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services
{
    [ExcludeFromCodeCoverage]
    public class KeyVaultService : IKeyVaultService
    {
        private readonly string keyVaultAddress;

        public KeyVaultService(string keyVaultAddress)
        {
            this.keyVaultAddress = keyVaultAddress;
        }

        public async Task<string> GetSecretAsync(string keyVaultKey)
        {
            var client = new SecretClient(new Uri(keyVaultAddress), new DefaultAzureCredential());
            var secret = await client.GetSecretAsync(keyVaultKey);

            return secret.Value.ToString();
        }
    }
}
