using Microsoft.Azure.Management.EventGrid;
using Microsoft.Azure.Management.EventGrid.Models;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services
{
    [ExcludeFromCodeCoverage]
    public class EventGridManagementClientWrapper : IEventGridManagementClientWrapper
    {
        private readonly IEventGridManagementClient client;

        public EventGridManagementClientWrapper(IEventGridManagementClient client)
        {
            this.client = client;
        }

        public async Task<EventSubscription> Subscription_CreateOrUpdateAsync(string scope, string eventSubscriptionName, EventSubscription subscription, CancellationToken cancellationToken = default)
        {
            return await client.EventSubscriptions.CreateOrUpdateAsync(scope, eventSubscriptionName, subscription, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Topic> Topic_GetAsync(string resourceGroupName, string topicName, CancellationToken cancellationToken = default)
        {
            return await client.Topics.GetAsync(resourceGroupName, topicName, cancellationToken);
        }

        public async Task Subscription_DeleteAsync(string scope, string eventSubscriptionName, CancellationToken cancellationToken = default)
        {
            await client.EventSubscriptions.DeleteAsync(scope, eventSubscriptionName, cancellationToken);
        }

    }
}
