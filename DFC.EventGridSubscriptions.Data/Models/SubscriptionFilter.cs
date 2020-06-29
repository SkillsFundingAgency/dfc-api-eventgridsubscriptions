
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Management.EventGrid.Models;

namespace DFC.EventGridSubscriptions.Data.Models
{
    [ExcludeFromCodeCoverage]
    public class SubscriptionFilter
    {
        public string? BeginsWith { get; set; }
        public string? EndsWith { get; set; }
        public List<string>? IncludeEventTypes { get; set; }
        public StringInAdvancedFilter? SubjectContainsFilter { get; set; }
    }
}
