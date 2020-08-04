using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DFC.EventGridSubscriptions.ApiFunction
{
    public static class DeadLetterEventGridTrigger
    {
        [FunctionName("ProcessDeadLetter")]
        public static void Run([EventGridTrigger]JObject eventGridEvent, ILogger log)
        {
            if (eventGridEvent == null)
            {
                throw new ArgumentNullException(nameof(eventGridEvent));
            }

            log.LogInformation($"Event grid trigger function has begun...");
            const string StorageBlobCreatedEvent = "Microsoft.Storage.BlobCreated";

            log.LogInformation(eventGridEvent.ToString(Formatting.Indented));
            var egEvent = eventGridEvent.ToObject<EventGridEvent>();

            if (egEvent != null && egEvent.Data != null)
            {
                JObject? dataObject = egEvent.Data as JObject;

                if (string.Equals(egEvent.EventType, StorageBlobCreatedEvent, StringComparison.OrdinalIgnoreCase))
                {
                    log.LogInformation("Received blob created event..");

                    var data = dataObject?.ToObject<StorageBlobCreatedEventData>();
                    log.LogInformation($"Dead Letter Blob Url:{data?.Url}");

                    log.LogInformation("Reading blob data from storage account..");
                    log.LogInformation($"Blob Data:{JsonConvert.SerializeObject(egEvent)}");
                }
            }
        }
    }
}
