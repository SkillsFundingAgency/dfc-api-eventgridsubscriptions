using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services.Interface
{
    public interface IKeyVaultService
    {
        Task<string> GetSecretAsync(string keyVaultKey);
    }
}
