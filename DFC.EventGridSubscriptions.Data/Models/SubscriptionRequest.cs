using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.EventGridSubscriptions.Data.Models
{
    public class SubscriptionRequest
    {
        public string? Name { get; set; }
        public Uri? Endpoint { get; set; }
        public SubscriptionFilter? Filter { get; set; }
    }
}
