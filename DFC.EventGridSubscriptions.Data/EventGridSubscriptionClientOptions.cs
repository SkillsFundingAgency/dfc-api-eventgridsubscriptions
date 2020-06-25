using System;

namespace DFC.EventGridSubscriptions.Data
{
    public class EventGridSubscriptionClientOptions
    {
        public string? ApplicationId { get; set; }
        public string? ClientSecret { get; set; }
        public string? TenantId { get; set; }
        public string? ResourceGroup { get; set; }
        public string? Topic { get; set; }
        public string? SubscriptionId { get; set; }
    }
}
