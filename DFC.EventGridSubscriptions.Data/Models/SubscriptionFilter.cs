using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.EventGridSubscriptions.Data.Models
{
    public class SubscriptionFilter
    {
        public string? BeginsWith { get; set; }
        public string? EndsWith { get; set; }
    }
}
