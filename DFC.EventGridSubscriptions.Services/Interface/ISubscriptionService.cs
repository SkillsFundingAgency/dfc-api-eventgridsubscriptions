using DFC.Compui.Subscriptions.Pkg.Data;
using Microsoft.Azure.Management.EventGrid.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services.Interface
{
    public interface ISubscriptionService
    {
        Task<HttpStatusCode> AddSubscription(SubscriptionSettings request);
        Task<HttpStatusCode> DeleteSubscription(string subscriptionName);
        Task<IEnumerable<EventSubscription>> GetAllSubscriptions();
        Task<EventSubscription> GetSubscription(string subscriptionName);
        Task<HttpStatusCode> StaleSubscription(string subscriberName);
    }
}
