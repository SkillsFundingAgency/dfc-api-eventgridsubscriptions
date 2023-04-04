using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.Management.EventGrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
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
            var app = ConfidentialClientApplicationBuilder
                .Create(configuration["dfc-api-eventgridsubscriptions-appregistration-id"])
                .WithClientSecret(configuration["dfc-api-eventgridsubscriptions-appregistration-secret"])
                .WithAuthority(new Uri("https://login.windows.net/" + configuration["dfc-api-eventgridsubscriptions-appregistration-tenant-id"]))
                .Build();

            try
            {
                var token = await app.AcquireTokenForClient(new string[] { ".default" })
                    .ExecuteAsync();
                return token.AccessToken;
            }
            catch (MsalUiRequiredException ex)
            {
                throw new MsalUiRequiredException(ex.ErrorCode, "The application doesn't have sufficient permissions.");
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                throw new MsalServiceException(ex.ErrorCode, "Invalid scope. The scope has to be in the form \"https://resourceurl/.default\"");
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token. Please verify the values for your applicationId, Password, and Tenant.");
            }
        }
    }
}
