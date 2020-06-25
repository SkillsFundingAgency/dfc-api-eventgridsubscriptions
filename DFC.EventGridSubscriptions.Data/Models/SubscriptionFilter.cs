
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.EventGridSubscriptions.Data.Models
{
    [ExcludeFromCodeCoverage]
    public class SubscriptionFilter
    {
        public string? BeginsWith { get; set; }
        public string? EndsWith { get; set; }
        public List<string>? IncludeEventTypes { get; set; }
    }
}
