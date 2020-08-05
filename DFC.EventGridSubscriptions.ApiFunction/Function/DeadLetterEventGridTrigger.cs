using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.ApiFunction
{
    public static class DeadLetterEventGridTrigger
    {
        [FunctionName("ProcessDeadLetter")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeadLetter/api/updates")] HttpRequestMessage req, ILogger log)
        {
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            log.LogInformation($"C# HTTP trigger function begun");
            string response = string.Empty;

            string requestContent = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
            log.LogInformation($"Received events: {requestContent}");

            EventGridSubscriber eventGridSubscriber = new EventGridSubscriber();

            EventGridEvent[] eventGridEvents = eventGridSubscriber.DeserializeEventGridEvents(requestContent);

            foreach (EventGridEvent eventGridEvent in eventGridEvents)
            {
                if (eventGridEvent.Data is SubscriptionValidationEventData)
                {
                    var eventData = (SubscriptionValidationEventData)eventGridEvent.Data;
                    log.LogInformation($"Got SubscriptionValidation event data, validation code: {eventData.ValidationCode}, topic: {eventGridEvent.Topic}");

                    // Do any additional validation (as required) and then return back the below response
                    var responseData = new SubscriptionValidationResponse()
                    {
                        ValidationResponse = eventData.ValidationCode,
                    };

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(responseData), Encoding.UTF8, "application/json"),
                    };
                }
                else if (eventGridEvent.Data is StorageBlobCreatedEventData)
                {
                    log.LogInformation($"Processing {nameof(StorageBlobCreatedEventData)} event started");

                    var eventData = (StorageBlobCreatedEventData)eventGridEvent.Data;

#pragma warning disable CA1304 // Specify CultureInfo
                    if (eventData.Url.ToLower().Contains("event-grid-dead-letter-events", StringComparison.InvariantCultureIgnoreCase))
#pragma warning restore CA1304 // Specify CultureInfo
                    {
                        log.LogInformation("Processing Dead Lettered Event");
                        log.LogInformation($"Dead Lettered Event Data: {JsonConvert.SerializeObject(eventData)}");
                    }

                    log.LogInformation($"Processing {nameof(StorageBlobCreatedEventData)} event completed");
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json"),
            };
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