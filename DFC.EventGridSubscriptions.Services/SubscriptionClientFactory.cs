using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.Management.EventGrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services
{
    [ExcludeFromCodeCoverage]
    public class SubscriptionClientFactory : ISubscriptionClientFactory
    {
        private readonly IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionOptions;
        private readonly IConfiguration configuration;

        public SubscriptionClientFactory(IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionOptions, IConfiguration configuration)
        {
            this.eventGridSubscriptionOptions = eventGridSubscriptionOptions;
            this.configuration = configuration;
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
            ClientCredential cc = new ClientCredential(configuration["dfc-api-eventgridsubscriptions-appregistration-id"], configuration["dfc-api-eventgridsubscriptions-appregistration-secret"]);
            var context = new AuthenticationContext("https://login.windows.net/" + configuration["dfc-api-eventgridsubscriptions-appregistration-tenant-id"]);
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
