using DFC.EventGridSubscriptions.Data.Models;
using Microsoft.Azure.Management.EventGrid.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services.Interface
{
    public interface ISubscriptionRegistrationService
    {
        Task<HttpStatusCode> AddSubscription(SubscriptionRequest request);
        Task<HttpStatusCode> DeleteSubscription(string subscriptionName);
        Task<IEnumerable<EventSubscription>> GetAllSubscriptions();
        Task<EventSubscription> GetSubscription(string subscriptionName);

    }
}
