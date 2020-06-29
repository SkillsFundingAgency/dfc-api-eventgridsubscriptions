using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Data.Models;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.Management.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services
{
    public class SubscriptionRegistrationService : ISubscriptionRegistrationService
    {
        private readonly IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionClientOptions;
        private readonly IEventGridManagementClientWrapper eventGridManagementClient;
        private readonly ILogger<SubscriptionRegistrationService> logger;

        public SubscriptionRegistrationService(IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionClientOptions, IEventGridManagementClientWrapper eventGridManagementClient, ILogger<SubscriptionRegistrationService> logger)
        {
            this.eventGridSubscriptionClientOptions = eventGridSubscriptionClientOptions;
            this.eventGridManagementClient = eventGridManagementClient;
            this.logger = logger;
        }

        public async Task<HttpStatusCode> AddSubscription(SubscriptionRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                if (string.IsNullOrEmpty(request.Name))
                {
                    throw new ArgumentException(nameof(request.Name));
                }

                if (request.Endpoint == null)
                {
                    throw new ArgumentException(nameof(request.Name));
                }

                logger.LogInformation($"{nameof(AddSubscription)} called for subscription: {request.Name}");

                await CreateEventGridEventSubscriptionAsync(request.Name!, request.Endpoint!.ToString(), request.Filter);

                return HttpStatusCode.Created;
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occured in {nameof(AddSubscription)} : {ex}");
                throw;
            }
        }

        public async Task<HttpStatusCode> DeleteSubscription(string subscriptionName)
        {
            try
            {
                if (string.IsNullOrEmpty(subscriptionName))
                {
                    throw new ArgumentNullException(nameof(subscriptionName));
                }

                logger.LogInformation($"{nameof(DeleteSubscription)} called for subscription: {subscriptionName}");

                await DeleteEventGridEventSubscriptionAsync(subscriptionName);

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occured in {nameof(DeleteSubscription)} : {ex}");
                throw;
            }
        }

        private async Task DeleteEventGridEventSubscriptionAsync(string subscriptionName)
        {
            Topic topic = await eventGridManagementClient.Topic_GetAsync(eventGridSubscriptionClientOptions!.CurrentValue!.ResourceGroup!, eventGridSubscriptionClientOptions!.CurrentValue!.Topic!);
            string eventSubscriptionScope = topic.Id;

            logger.LogInformation($"Deleting subscription {subscriptionName} from topic {topic.Name}...");

            await eventGridManagementClient.Subscription_DeleteAsync(eventSubscriptionScope, subscriptionName);

            logger.LogInformation("EventGrid event subscription deleted with name " + subscriptionName);
        }

        private async Task CreateEventGridEventSubscriptionAsync(string eventSubscriptionName, string endpointUrl, SubscriptionFilter? filter)
        {
            Topic topic = await eventGridManagementClient.Topic_GetAsync(eventGridSubscriptionClientOptions!.CurrentValue!.ResourceGroup!, eventGridSubscriptionClientOptions!.CurrentValue!.Topic!);
            string eventSubscriptionScope = topic.Id;

            logger.LogInformation($"Creating an event subscription to topic {topic.Name}...");

            EventSubscription eventSubscription = new EventSubscription()
            {
                Destination = new WebHookEventSubscriptionDestination()
                {
                    EndpointUrl = endpointUrl
                },
                EventDeliverySchema = EventDeliverySchema.EventGridSchema,
                Filter = filter != null ? new EventSubscriptionFilter()
                {
                    IsSubjectCaseSensitive = false,
                    SubjectBeginsWith = filter.BeginsWith ?? "",
                    SubjectEndsWith = filter.EndsWith ?? "",
                    IncludedEventTypes = filter.IncludeEventTypes ?? null,
                    AdvancedFilters = BuildAdvancedFilters(filter),
                } : new EventSubscriptionFilter()
            };

            EventSubscription createdEventSubscription = await eventGridManagementClient.Subscription_CreateOrUpdateAsync(eventSubscriptionScope, eventSubscriptionName, eventSubscription);
            logger.LogInformation("EventGrid event subscription created with name " + createdEventSubscription.Name);
        }

        private IList<AdvancedFilter> BuildAdvancedFilters(SubscriptionFilter filter)
        {
            if(filter.PropertyContainsFilter == null)
            {
                return new List<AdvancedFilter>();
            }

            var filterList = new List<AdvancedFilter>();

            filterList.Add(filter.PropertyContainsFilter);

            return filterList;
        }
    }
}
