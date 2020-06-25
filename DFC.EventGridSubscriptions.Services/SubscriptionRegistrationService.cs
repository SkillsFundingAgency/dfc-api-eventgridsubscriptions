using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Data.Models;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.Management.EventGrid;
using Microsoft.Azure.Management.EventGrid.Models;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services
{
    public class SubscriptionRegistrationService : ISubscriptionRegistrationService
    {
        private readonly IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionClientOptions;
        private readonly IEventGridManagementClient eventGridManagementClient;
        private readonly ILogger<SubscriptionRegistrationService> logger;

        public SubscriptionRegistrationService(IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionClientOptions, IEventGridManagementClient eventGridManagementClient, ILogger<SubscriptionRegistrationService> logger)
        {
            this.eventGridSubscriptionClientOptions = eventGridSubscriptionClientOptions;
            this.eventGridManagementClient = eventGridManagementClient;
            this.logger = logger;
        }

        public async Task<HttpStatusCode> AddSubscription(SubscriptionRequest request)
        {
            try
            {
                this.ValidateRequest(request);

                logger.LogInformation($"{nameof(AddSubscription)} called for subscription: {request.Name}");

                await CreateEventGridEventSubscriptionAsync(request!.Name!, eventGridManagementClient, request!.Endpoint!.ToString(), request.Filter);

                return HttpStatusCode.Created;
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occured in {nameof(AddSubscription)} : {ex}");
                return HttpStatusCode.InternalServerError;
            }
        }

        private void ValidateRequest(SubscriptionRequest request)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new ArgumentException(nameof(request.Name));
            }

            if(request.Endpoint == null)
            {
                throw new ArgumentException(nameof(request.Endpoint));
            }
        }

        public async Task<HttpStatusCode> DeleteSubscription(string subscriptionName)
        {
            try
            {
                logger.LogInformation($"{nameof(DeleteSubscription)} called for subscription: {subscriptionName}");

                await DeleteEventGridEventSubscriptionAsync(subscriptionName, eventGridManagementClient);

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occured in {nameof(DeleteSubscription)} : {ex}");
                return HttpStatusCode.InternalServerError;
            }
        }

        private async Task DeleteEventGridEventSubscriptionAsync(string subscriptionName, IEventGridManagementClient eventGridManagementClient)
        {
            Topic topic = await eventGridManagementClient.Topics.GetAsync(eventGridSubscriptionClientOptions.CurrentValue.ResourceGroup, eventGridSubscriptionClientOptions.CurrentValue.Topic);
            string eventSubscriptionScope = topic.Id;

            logger.LogInformation($"Deleting subscription {subscriptionName} from topic {topic.Name}...");

            await eventGridManagementClient.EventSubscriptions.DeleteAsync(eventSubscriptionScope, subscriptionName);

            logger.LogInformation("EventGrid event subscription deleted with name " + subscriptionName);
        }

        private async Task CreateEventGridEventSubscriptionAsync(string eventSubscriptionName, IEventGridManagementClient eventGridMgmtClient, string endpointUrl, SubscriptionFilter? filter)
        {
            Topic topic = await eventGridMgmtClient.Topics.GetAsync(eventGridSubscriptionClientOptions.CurrentValue.ResourceGroup, eventGridSubscriptionClientOptions.CurrentValue.Topic);
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
                    IncludedEventTypes = filter.IncludeEventTypes ?? null
                } : new EventSubscriptionFilter()
            };

            EventSubscription createdEventSubscription = await eventGridMgmtClient.EventSubscriptions.CreateOrUpdateAsync(eventSubscriptionScope, eventSubscriptionName, eventSubscription);
            logger.LogInformation("EventGrid event subscription created with name " + createdEventSubscription.Name);
        }
    }
}
