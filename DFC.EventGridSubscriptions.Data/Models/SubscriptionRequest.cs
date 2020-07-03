using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.EventGridSubscriptions.Data.Models
{
    [ExcludeFromCodeCoverage]
    public class SubscriptionRequest
    {
        public string? Name { get; set; }
        public Uri? Endpoint { get; set; }
        public SubscriptionFilter? Filter { get; set; }
    }
}
