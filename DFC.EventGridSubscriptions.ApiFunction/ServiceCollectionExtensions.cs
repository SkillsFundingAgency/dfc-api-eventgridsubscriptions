using DFC.EventGridSubscriptions.Services;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.Management.EventGrid;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddTransient<IEventGridManagementClientWrapper, EventGridManagementClientWrapper>();
            services.AddSingleton<ISubscriptionClientFactory, SubscriptionClientFactory>();
            services.AddTransient<IEventGridManagementClient, EventGridManagementClient>(sp => { return GetClient(sp).GetAwaiter().GetResult(); });
        }

        private static async Task<EventGridManagementClient> GetClient(IServiceProvider serviceProvider)
        {
            var subscriptionClientFactory = serviceProvider.GetRequiredService<ISubscriptionClientFactory>();
            return await subscriptionClientFactory.CreateClient().ConfigureAwait(false);
        }
    }
}
