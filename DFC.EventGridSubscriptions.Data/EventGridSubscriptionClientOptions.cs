using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.EventGridSubscriptions.Data
{
    [ExcludeFromCodeCoverage]
    public class EventGridSubscriptionClientOptions
    {
        public string? ResourceGroup { get; set; }
        public string? TopicName { get; set; }
        public string? SubscriptionId { get; set; }
        public string? DeadLetterBlobContainerName { get; set; }
        public string? DeadLetterBlobResourceId { get; set; }
        public int? RetryPolicyEventTimeToLiveInMinutes { get; set; }
        public int? RetryPolicyMaxDeliveryAttempts { get; set; }
        public TimeSpan? StaleSubscriptionInterval { get; set; }
        public int StaleSubscriptionThreshold { get; set; }
    }
}
