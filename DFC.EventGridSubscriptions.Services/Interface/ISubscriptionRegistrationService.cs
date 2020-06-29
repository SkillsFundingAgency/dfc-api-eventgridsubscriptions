using DFC.EventGridSubscriptions.Data.Models;
using System.Net;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services.Interface
{
    public interface ISubscriptionRegistrationService
    {
        Task<HttpStatusCode> AddSubscription(SubscriptionRequest request);
        Task<HttpStatusCode> DeleteSubscription(string subscriptionName);
    }
}
