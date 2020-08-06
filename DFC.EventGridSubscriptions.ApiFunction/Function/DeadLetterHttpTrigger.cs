using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.ApiFunction
{
    public class DeadLetterHttpTrigger
    {
        private readonly IOptionsMonitor<EventGridSubscriptionClientOptions> options;
        private readonly ISubscriptionService subscriptionService;

        public DeadLetterHttpTrigger(IOptionsMonitor<EventGridSubscriptionClientOptions> options, ISubscriptionService subscriptionService)
        {
            this.options = options;
            this.subscriptionService = subscriptionService;
        }

        [FunctionName("ProcessDeadLetter")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeadLetter/api/updates")] HttpRequestMessage req, ILogger log)
        {
            if (Activity.Current == null)
            {
                Activity.Current = new Activity($"{nameof(DeadLetterHttpTrigger)}").Start();
            }

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
                    if (options.CurrentValue.DeadLetterBlobContainerName == null)
                    {
                        throw new ArgumentException(nameof(options.CurrentValue.DeadLetterBlobContainerName));
                    }

                    log.LogInformation($"Processing {nameof(StorageBlobCreatedEventData)} event started");

                    var eventData = (StorageBlobCreatedEventData)eventGridEvent.Data;

                    if (eventData == null)
                    {
                        throw new InvalidDataException($"{nameof(StorageBlobCreatedEventData)} in EventGridEvent {eventGridEvent.Id} is null");
                    }

                    if (eventData.Url != null && eventData.Url!.ToUpperInvariant().Contains(options.CurrentValue.DeadLetterBlobContainerName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        log.LogInformation("Processing Dead Lettered Event");

                        var blobString = $"{options.CurrentValue.DeadLetterBlobContainerName}/{options.CurrentValue.TopicName}/";

                        int startIndex = eventData.Url.IndexOf(blobString, StringComparison.OrdinalIgnoreCase) + blobString.Length;
                        int endIndex = eventData.Url.IndexOf("/", startIndex, StringComparison.OrdinalIgnoreCase);
                        var subscriberName = eventData.Url[startIndex..endIndex];

                        log.LogError($"Dead Lettered Event, Blob URL: {eventData.Url}, SubscriberName {subscriberName}");

                        var result = await subscriptionService.StaleSubscription(subscriberName).ConfigureAwait(false);
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