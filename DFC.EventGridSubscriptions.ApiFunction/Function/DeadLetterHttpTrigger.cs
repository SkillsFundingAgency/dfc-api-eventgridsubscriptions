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
            Initialise(req);

            log.LogInformation($"C# HTTP trigger function begun");
            string response = string.Empty;

            string requestContent = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
            log.LogInformation($"Received events: {requestContent}");

            EventGridSubscriber eventGridSubscriber = new EventGridSubscriber();

            EventGridEvent[] eventGridEvents = eventGridSubscriber.DeserializeEventGridEvents(requestContent);

            foreach (EventGridEvent eventGridEvent in eventGridEvents)
            {
                if (eventGridEvent.Data.GetType() == typeof(SubscriptionValidationEventData))
                {
                    var eventData = eventGridEvent.Data as SubscriptionValidationEventData;

                    if (eventData == null)
                    {
                        throw new InvalidDataException($"{nameof(SubscriptionValidationEventData)} in EventGridEvent {eventGridEvent.Id} is null");
                    }

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
                else if (eventGridEvent.Data.GetType() == typeof(StorageBlobCreatedEventData))
                {
                    if (options.CurrentValue.DeadLetterBlobContainerName == null)
                    {
                        throw new ArgumentException(nameof(options.CurrentValue.DeadLetterBlobContainerName));
                    }

                    var eventData = eventGridEvent.Data as StorageBlobCreatedEventData;

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
                        return new HttpResponseMessage(result);
                    }
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json"),
            };
        }

        private static void Initialise(HttpRequestMessage req)
        {
            if (Activity.Current == null)
            {
                Activity.Current = new Activity($"{nameof(DeadLetterHttpTrigger)}").Start();
            }

            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }
        }
    }
}