using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services
{
    public class KeyVaultService : IKeyVaultService
    {
        private readonly string keyVaultAddress;

        public KeyVaultService(string keyVaultAddress)
        {
            this.keyVaultAddress = keyVaultAddress;
        }

        public async Task<string> GetSecretAsync(string key)
        {
            var azureServiceTokenProvider1 = new AzureServiceTokenProvider();
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider1.KeyVaultTokenCallback));
            var secret = await kv.GetSecretAsync(keyVaultAddress, key);

            return secret.Value;
        }
    }
}
