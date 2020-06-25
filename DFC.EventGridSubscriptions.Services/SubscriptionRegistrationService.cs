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
using System.Net;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services
{
    public class SubscriptionRegistrationService : ISubscriptionRegistrationService
    {
        private readonly IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionClientOptions;
        private readonly ILogger<SubscriptionRegistrationService> logger;

        public SubscriptionRegistrationService(IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionClientOptions, ILogger<SubscriptionRegistrationService> logger)
        {
            this.eventGridSubscriptionClientOptions = eventGridSubscriptionClientOptions;
            this.logger = logger;
        }

        public async Task<HttpStatusCode> AddSubscription(SubscriptionRequest request)
        {
            try
            {
                this.ValidateRequest(request);

                logger.LogInformation($"{nameof(AddSubscription)} called for subscription: {request.Name}");

                EventGridManagementClient eventGridManagementClient = await CreateEventGridManagementClient();

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

                EventGridManagementClient eventGridManagementClient = await CreateEventGridManagementClient();

                await DeleteEventGridEventSubscriptionAsync(subscriptionName, eventGridManagementClient);

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occured in {nameof(DeleteSubscription)} : {ex}");
                return HttpStatusCode.InternalServerError;
            }
        }

        private Task DeleteEventGridEventSubscriptionAsync(string subscriptionName, EventGridManagementClient eventGridManagementClient)
        {
            throw new NotImplementedException();
        }

        private async Task<EventGridManagementClient> CreateEventGridManagementClient()
        {
            string token = await GetAuthorizationHeaderAsync();
            TokenCredentials credential = new TokenCredentials(token);

            EventGridManagementClient eventGridManagementClient = new EventGridManagementClient(credential)
            {
                SubscriptionId = eventGridSubscriptionClientOptions.CurrentValue.SubscriptionId
            };

            return eventGridManagementClient;
        }

        private async Task<string> GetAuthorizationHeaderAsync()
        {
            ClientCredential cc = new ClientCredential(eventGridSubscriptionClientOptions.CurrentValue.ApplicationId, eventGridSubscriptionClientOptions.CurrentValue.ClientSecret);
            var context = new AuthenticationContext("https://login.windows.net/" + eventGridSubscriptionClientOptions.CurrentValue.TenantId);
            var result = await context.AcquireTokenAsync("https://management.azure.com/", cc);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token. Please verify the values for your applicationId, Password, and Tenant.");
            }

            string token = result.AccessToken;
            return token;

        }

        private async Task CreateEventGridEventSubscriptionAsync(string eventSubscriptionName, EventGridManagementClient eventGridMgmtClient, string endpointUrl, SubscriptionFilter? filter)
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
                    SubjectEndsWith = filter.EndsWith ?? ""
                } : new EventSubscriptionFilter()
            };

            EventSubscription createdEventSubscription = await eventGridMgmtClient.EventSubscriptions.CreateOrUpdateAsync(eventSubscriptionScope, eventSubscriptionName, eventSubscription);
            logger.LogInformation("EventGrid event subscription created with name " + createdEventSubscription.Name);
        }
    }
}
