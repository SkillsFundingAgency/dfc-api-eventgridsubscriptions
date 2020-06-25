using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.Management.EventGrid;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services
{
    public class SubscriptionClientFactory : ISubscriptionClientFactory
    {
        private readonly IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionOptions;

        public SubscriptionClientFactory(IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionOptions)
        {
            this.eventGridSubscriptionOptions = eventGridSubscriptionOptions;
        }

        public async Task<EventGridManagementClient> CreateClient()
        {
            return await CreateEventGridManagementClient();
        }

        private async Task<EventGridManagementClient> CreateEventGridManagementClient()
        {
            string token = await GetAuthorizationHeaderAsync().ConfigureAwait(false);
            TokenCredentials credential = new TokenCredentials(token);

            EventGridManagementClient eventGridManagementClient = new EventGridManagementClient(credential)
            {
                SubscriptionId = eventGridSubscriptionOptions.CurrentValue.SubscriptionId,
            };

            return eventGridManagementClient;
        }

        private async Task<string> GetAuthorizationHeaderAsync()
        {
            ClientCredential cc = new ClientCredential(eventGridSubscriptionOptions.CurrentValue.ApplicationId, eventGridSubscriptionOptions.CurrentValue.ClientSecret);
            var context = new AuthenticationContext("https://login.windows.net/" + eventGridSubscriptionOptions.CurrentValue.TenantId);
            var result = await context.AcquireTokenAsync("https://management.azure.com/", cc).ConfigureAwait(false);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token. Please verify the values for your applicationId, Password, and Tenant.");
            }

            string token = result.AccessToken;
            return token;
        }
    }
}
