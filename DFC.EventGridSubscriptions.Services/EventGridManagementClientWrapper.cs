﻿using Microsoft.Azure.Management.EventGrid;
using Microsoft.Azure.Management.EventGrid.Models;
using System;
using System.Collections.Generic;
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

        public async Task<IEnumerable<EventSubscription>> Subscription_GetAllAsync(string resourceGroupName, string topicName, CancellationToken cancellationToken = default)
        {
            //SDK appears out of line with web reference:
            //https://docs.microsoft.com/en-us/rest/api/eventgrid/version2020-06-01/eventsubscriptions/listbyresource
            var result = await client.EventSubscriptions.ListByResourceAsync(resourceGroupName, string.Empty, "Microsoft.EventGrid", "/topics/" + topicName, null, null, cancellationToken);
            return result;
        }

        public async Task<EventSubscription> Subscription_GetByIdAsync(string scope, string subscriptionName, CancellationToken cancellationToken = default)
        {   
            var result = await client.EventSubscriptions.GetAsync(scope, subscriptionName, cancellationToken);
            return result;
        }
    }
}
