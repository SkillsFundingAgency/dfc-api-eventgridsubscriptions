using Castle.Core.Logging;
using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services;
using FakeItEasy;
using Microsoft.Azure.Management.EventGrid;
using Microsoft.Azure.Management.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DFC.EventGridSubscriptions.ApiFunction.UnitTests.DFC.EventGridSubscriptions.Services.Tests
{
    public class SubscriptionRegistrationServiceTests
    {
        private readonly IOptionsMonitor<EventGridSubscriptionClientOptions> fakeClientOptions = A.Fake<IOptionsMonitor<EventGridSubscriptionClientOptions>>();
        private readonly ILogger<SubscriptionRegistrationService> fakeLogger = A.Fake<ILogger<SubscriptionRegistrationService>>();
        private readonly IEventGridManagementClientWrapper fakeClient = A.Fake<IEventGridManagementClientWrapper>();

        [Fact]
        public async Task DoSomething()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync.GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeClient, fakeLogger);

            //Act
            var result = await serviceToTest.AddSubscription(new Data.Models.SubscriptionRequest { Endpoint = new Uri("http://somehost.com/awebhook"), Name = "Test Subscriber" });

            //Assert
            Assert.Equal(HttpStatusCode.Created, result);
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeClient.Subscription_CreateOrUpdateAsync(A<string>.Ignored, A<string>.Ignored, A<EventSubscription>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
