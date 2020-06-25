using DFC.EventGridSubscriptions.Data;
using Microsoft.Azure.Management.EventGrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.ApiFunction
{
    /// <summary>
    /// The Service Collection Extensions Class.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Event Grid Management Client.
        /// </summary>
        /// <param name="services">The Service Provider.</param>
        public static void AddEventGridManagementClient(this IServiceCollection services)
        {
            services.AddTransient<IEventGridManagementClient, EventGridManagementClient>(sp => { return GenerateEventGridManagementClient(sp).GetAwaiter().GetResult(); });
        }

        private static async Task<EventGridManagementClient> GenerateEventGridManagementClient(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IOptionsMonitor<EventGridSubscriptionClientOptions>>();

            return await CreateEventGridManagementClient(configuration.CurrentValue).ConfigureAwait(false);
        }

        private static async Task<EventGridManagementClient> CreateEventGridManagementClient(EventGridSubscriptionClientOptions eventGridSubscriptionClientOptions)
        {
            string token = await GetAuthorizationHeaderAsync(eventGridSubscriptionClientOptions).ConfigureAwait(false);
            TokenCredentials credential = new TokenCredentials(token);

            EventGridManagementClient eventGridManagementClient = new EventGridManagementClient(credential)
            {
                SubscriptionId = eventGridSubscriptionClientOptions.SubscriptionId,
            };

            return eventGridManagementClient;
        }

        private static async Task<string> GetAuthorizationHeaderAsync(EventGridSubscriptionClientOptions eventGridSubscriptionClientOptions)
        {
            ClientCredential cc = new ClientCredential(eventGridSubscriptionClientOptions.ApplicationId, eventGridSubscriptionClientOptions.ClientSecret);
            var context = new AuthenticationContext("https://login.windows.net/" + eventGridSubscriptionClientOptions.TenantId);
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
