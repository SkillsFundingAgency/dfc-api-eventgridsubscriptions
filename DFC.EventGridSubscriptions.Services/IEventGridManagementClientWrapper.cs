using Microsoft.Azure.Management.EventGrid.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services
{
    public interface IEventGridManagementClientWrapper
    {
        Task<EventSubscription> Subscription_CreateOrUpdateAsync(string scope, string eventSubscriptionName, EventSubscription subscription, CancellationToken cancellationToken = default);
        Task<Topic> Topic_GetAsync(string resourceGroupName, string topicName, CancellationToken cancellationToken = default);
        Task Subscription_DeleteAsync(string scope, string eventSubscriptionName, CancellationToken cancellationToken = default);
        Task<IEnumerable<EventSubscription>> Subscription_GetAllAsync(string resourceGroupName, string topicName, CancellationToken cancellationToken = default);
    }
}
