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
        public string DeadLetterBlobResourceId { get; set; }
    }
}
