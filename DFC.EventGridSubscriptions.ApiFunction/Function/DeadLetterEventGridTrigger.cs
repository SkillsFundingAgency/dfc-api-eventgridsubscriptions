using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace DFC.EventGridSubscriptions.ApiFunction
{
    public static class DeadLetterEventGridTrigger
    {
        [FunctionName("ProcessDeadLetter")]
        public static void Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeadLetter/api/updates")] HttpRequest req, ILogger log)
        {
            var body = req.ReadAsStringAsync();
            log.LogInformation($"C# ProcessDeadLetter Trigger Fired. Body:{body}");
        }

        //if (eventGridEvent == null)
        //{
        //    throw new ArgumentNullException(nameof(eventGridEvent));
        //}

        //log.LogInformation($"Event grid trigger function has begun...");
        //const string StorageBlobCreatedEvent = "Microsoft.Storage.BlobCreated";

        //log.LogInformation(eventGridEvent.ToString(Formatting.Indented));
        //var egEvent = eventGridEvent.ToObject<EventGridEvent>();

        //log.LogInformation($"Event is:{egEvent}");
        //if (egEvent != null && egEvent.Data != null)
        //{
        //    log.LogInformation("Event and Data Not Null...");
        //    JObject? dataObject = egEvent.Data as JObject;

        //    log.LogInformation($"Event Type: {egEvent.EventType}");

        //    if (string.Equals(egEvent.EventType, StorageBlobCreatedEvent, StringComparison.OrdinalIgnoreCase))
        //    {
        //        log.LogInformation("Received blob created event..");

        //        var data = dataObject?.ToObject<StorageBlobCreatedEventData>();
        //        log.LogInformation($"Dead Letter Blob Url:{data?.Url}");

        //        log.LogInformation("Reading blob data from storage account..");
        //        log.LogInformation($"Blob Data:{JsonConvert.SerializeObject(egEvent)}");

        //        ProcessBlob(data!.Url, log);
        //    }
        //}
    }

    /*
    This function uses the blob url/location received in the BlobCreated event to fetch data from the storage container.
    Here, we perform a simple GET request on the blob url and deserialize the dead lettered events in a json array.
    */

    //public static void ProcessBlob(string url, ILogger log)
    //{
    //    // sas key generated through the portal for your storage account used for authentication
    //    const string sasKey = "--replace-with-the-storage-account-sas-key";
    //    string uri = url + sasKey;

    //    WebClient client = new WebClient();

    //    Stream data = client.OpenRead(uri);
    //    StreamReader reader = new StreamReader(data);
    //    string blob = reader.ReadToEnd();
    //    data.Close();
    //    reader.Close();

    //    log.LogInformation($"Dead Letter Events:{blob}");

    //    // deserialize the blob into dead letter events
    //    DeadLetterEvent[] dlEvents = JsonConvert.DeserializeObject<DeadLetterEvent[]>(blob);

    //    foreach (DeadLetterEvent dlEvent in dlEvents)
    //    {
    //        log.LogInformation($"Printing Dead Letter Event attributes for EventId: {dlEvent.Id}, Dead Letter Reason:{dlEvent.DeadLetterReason}, DeliveryAttempts:{dlEvent.DeliveryAttempts}, LastDeliveryOutcome:{dlEvent.LastDeliveryOutcome}, LastHttpStatusCode:{dlEvent.LastHttpStatusCode}");
    //    }

    //    client.Dispose();
    //}
}