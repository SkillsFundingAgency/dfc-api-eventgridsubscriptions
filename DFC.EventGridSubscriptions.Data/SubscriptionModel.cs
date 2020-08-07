using DFC.Compui.Cosmos.Contracts;
using DFC.Compui.Telemetry.Models;
using DFC.EventGridSubscriptions.Data.Enums;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.EventGridSubscriptions.Data
{
    [ExcludeFromCodeCoverage]
    public class SubscriptionModel : RequestTrace, IDocumentModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("_etag")]
        public string? Etag { get; set; }
        public string? PartitionKey { get; set; }
        public string? Name { get; set; }
        public DateTime? LastModified { get; set; }
        public int StaleCount { get; set; }
        public DateTime? LastStale { get; set; }
        public SubscriptionStatus Status { get; set; }
    }
}
