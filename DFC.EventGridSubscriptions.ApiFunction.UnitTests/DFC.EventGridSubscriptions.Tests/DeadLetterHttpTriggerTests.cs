using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services.Interface;
using FakeItEasy;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.EventGridSubscriptions.ApiFunction.UnitTests.DFC.EventGridSubscriptions.Tests
{
    public class DeadLetterHttpTriggerTests
    {
        private readonly DeadLetterHttpTrigger _executeFunction;
        private readonly ILogger _log;
        private readonly ISubscriptionService subscriptionRegistrationService;
        private readonly IOptionsMonitor<EventGridSubscriptionClientOptions> eventGridSubscriptionClientOptions;

        public DeadLetterHttpTriggerTests()
        {
            subscriptionRegistrationService = A.Fake<ISubscriptionService>();

            eventGridSubscriptionClientOptions = A.Fake<IOptionsMonitor<EventGridSubscriptionClientOptions>>();

            _log = A.Fake<ILogger>();

            _executeFunction = new DeadLetterHttpTrigger(eventGridSubscriptionClientOptions, subscriptionRegistrationService);
        }

        [Fact]
        public async Task DeadLetterHttpTriggerNullRequestThrowsException()
        {
            // Arrange
            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await RunFunction(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task DeadLetterHttpTriggerWhenPassedValidationEventReturnsCode()
        {
            // Arrange
            string expectedValidationCode = Guid.NewGuid().ToString();
            var eventGridEvents = BuildValidEventGridEvent(Microsoft.Azure.EventGrid.EventTypes.EventGridSubscriptionValidationEvent, new SubscriptionValidationEventData(expectedValidationCode, "https://somewhere.com"));


            // Act
            var result = await RunFunction(new HttpRequestMessage { Content = new StringContent(JsonConvert.SerializeObject(eventGridEvents)) });

            // Assert
            Assert.Equal(200, (int)result.StatusCode);
            var responseResult = Assert.IsType<HttpResponseMessage>(result);
            var response = JsonConvert.DeserializeObject<SubscriptionValidationResponse>(await responseResult.Content.ReadAsStringAsync());

            Assert.Equal(expectedValidationCode, response.ValidationResponse);
        }

        [Fact]
        public async Task DeadLetterHttpTriggerWhenPassedDeadLetterEventReturnsOk()
        {
            // Arrange
            A.CallTo(() => eventGridSubscriptionClientOptions.CurrentValue).Returns(new EventGridSubscriptionClientOptions { DeadLetterBlobContainerName = "event-grid-dead-letter-events", TopicName = "dfc-dev-stax-egt" });
            A.CallTo(() => subscriptionRegistrationService.StaleSubscription(A<string>.Ignored)).Returns(HttpStatusCode.OK);

            string expectedValidationCode = Guid.NewGuid().ToString();
            var eventGridEvents = BuildValidEventGridEvent(Microsoft.Azure.EventGrid.EventTypes.StorageBlobCreatedEvent, new StorageBlobCreatedEventData() { Url = "https://dfcdevcompuisharedstr.blob.core.windows.net/event-grid-dead-letter-events/dfc-dev-stax-egt/TEST-SUBSCRIPTION-CONTACTUS-TEST/2020/8/6/9/76d47aaa-be54-495e-993f-4bb1ba65cddb.json" });

            // Act
            var result = await RunFunction(new HttpRequestMessage { Content = new StringContent(JsonConvert.SerializeObject(eventGridEvents)) });

            // Assert
            Assert.Equal(200, (int)result.StatusCode);
            A.CallTo(() => subscriptionRegistrationService.StaleSubscription(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }


        private async Task<HttpResponseMessage> RunFunction(HttpRequestMessage message)
        {
            return await _executeFunction.Run(message, _log).ConfigureAwait(false);
        }

        protected static EventGridEvent[] BuildValidEventGridEvent<TModel>(string eventType, TModel data)
        {
            var models = new EventGridEvent[]
            {
                new EventGridEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Subject = "test/a-canonical-name",
                    Data = data,
                    EventType = eventType,
                    EventTime = DateTime.Now,
                    DataVersion = "1.0",
                },
            };

            return models;
        }
    }
}
