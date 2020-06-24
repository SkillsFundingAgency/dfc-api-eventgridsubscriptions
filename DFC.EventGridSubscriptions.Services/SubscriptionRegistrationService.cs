using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.Management.EventGrid;
using Microsoft.Azure.Management.EventGrid.Models;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services
{
    public class SubscriptionRegistrationService : ISubscriptionRegistrationService
    {
        private readonly IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionClientOptions;

        public SubscriptionRegistrationService(IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionClientOptions)
        {
            this.eventGridSubscriptionClientOptions = eventGridSubscriptionClientOptions;
        }

        public async Task<string> AddSubscription()
        {
            EventGridManagementClient eventGridManagementClient = await CreateEventGridManagementClient();

            await CreateEventGridEventSubscriptionAsync("my-test-subscription", eventGridManagementClient, "https://webhook.site/337754e2-0efe-4496-aa7a-a40babb80ba3");
            return "";
        }

        private async Task<EventGridManagementClient> CreateEventGridManagementClient()
        {
            string token = await GetAuthorizationHeaderAsync();
            TokenCredentials credential = new TokenCredentials(token);

            EventGridManagementClient eventGridManagementClient = new EventGridManagementClient(credential)
            {
                SubscriptionId = eventGridSubscriptionClientOptions.CurrentValue.SubscriptionId,
                LongRunningOperationRetryTimeout = 2
            };
            return eventGridManagementClient;
        }

        public async Task<string> DeleteSubscription()
        {
            return string.Empty;
        }

        //The following method will enable you to use the token to create credentials
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

        private async Task CreateEventGridEventSubscriptionAsync(string eventSubscriptionName, EventGridManagementClient eventGridMgmtClient, string endpointUrl)
        {
            Topic topic = await eventGridMgmtClient.Topics.GetAsync(eventGridSubscriptionClientOptions.CurrentValue.ResourceGroup, eventGridSubscriptionClientOptions.CurrentValue.Topic);
            string eventSubscriptionScope = topic.Id;

            Console.WriteLine($"Creating an event subscription to topic {topic.Name}...");

            EventSubscription eventSubscription = new EventSubscription()
            {
                Destination = new WebHookEventSubscriptionDestination()
                {
                    EndpointUrl = endpointUrl
                },
                // The below are all optional settings
                EventDeliverySchema = EventDeliverySchema.EventGridSchema,
                Filter = new EventSubscriptionFilter()
                {
                    // By default, "All" event types are included
                    IsSubjectCaseSensitive = false,
                    SubjectBeginsWith = "",
                    SubjectEndsWith = ""
                }
            };

            EventSubscription createdEventSubscription = await eventGridMgmtClient.EventSubscriptions.CreateOrUpdateAsync(eventSubscriptionScope, eventSubscriptionName, eventSubscription);
            Console.WriteLine("EventGrid event subscription created with name " + createdEventSubscription.Name);
        }

        public Task<string> DeleteSubcription()
        {
            throw new NotImplementedException();
        }
    }
}
