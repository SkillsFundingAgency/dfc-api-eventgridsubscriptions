using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services;
using FakeItEasy;
using Microsoft.Azure.Management.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public async Task SubscriptionRegistrationServiceWhenAddSubscriptionAddsSubscription()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeClient, fakeLogger);

            //Act
            var result = await serviceToTest.AddSubscription(new Data.Models.SubscriptionRequest { Endpoint = new Uri("http://somehost.com/awebhook"), Name = "Test Subscriber" });

            //Assert
            Assert.Equal(HttpStatusCode.Created, result);
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeClient.Subscription_CreateOrUpdateAsync(A<string>.Ignored, A<string>.Ignored, A<EventSubscription>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenAddSubscription5AdvancedFiltersAddsSubscription()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeClient, fakeLogger);

            //Act
            var result = await serviceToTest.AddSubscription(new Data.Models.SubscriptionRequest { Endpoint = new Uri("http://somehost.com/awebhook"), Name = "Test Subscriber", Filter = new Data.Models.SubscriptionFilter { PropertyContainsFilters = new List<StringContainsAdvancedFilter> { new StringContainsAdvancedFilter("subject", new List<string> { "a", "b", "c", "d", "e" }) } } });

            //Assert
            Assert.Equal(HttpStatusCode.Created, result);
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeClient.Subscription_CreateOrUpdateAsync(A<string>.Ignored, A<string>.Ignored, A<EventSubscription>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenAddSubscriptioAdvancedFiltersGreaterThan5ThrowsException()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeClient, fakeLogger);

            //Act
            //Assert
            await Assert.ThrowsAsync<ArgumentException>(() => serviceToTest.AddSubscription(new Data.Models.SubscriptionRequest { Endpoint = new Uri("http://somehost.com/awebhook"), Name = null, Filter = new Data.Models.SubscriptionFilter { PropertyContainsFilters = new List<StringContainsAdvancedFilter> { new StringContainsAdvancedFilter("subject", new List<string> { "1", "2", "3", "4", "5" }) } } }));
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeClient.Subscription_CreateOrUpdateAsync(A<string>.Ignored, A<string>.Ignored, A<EventSubscription>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenAddSubscriptionNullNameThrowsException()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeClient, fakeLogger);

            //Act
            //Assert
            await Assert.ThrowsAsync<ArgumentException>(() => serviceToTest.AddSubscription(new Data.Models.SubscriptionRequest { Endpoint = new Uri("http://somehost.com/awebhook"), Name = null }));
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeClient.Subscription_CreateOrUpdateAsync(A<string>.Ignored, A<string>.Ignored, A<EventSubscription>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenAddSubscriptionNullEndpointThrowsException()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeClient, fakeLogger);

            //Act
            //Assert
            await Assert.ThrowsAsync<ArgumentException>(() => serviceToTest.AddSubscription(new Data.Models.SubscriptionRequest { Endpoint = null, Name = "a-test-subscription" }));
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeClient.Subscription_CreateOrUpdateAsync(A<string>.Ignored, A<string>.Ignored, A<EventSubscription>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenAddSubscriptionNullRequestThrowsException()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeClient, fakeLogger);

            //Act
            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => serviceToTest.AddSubscription(null));
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeClient.Subscription_CreateOrUpdateAsync(A<string>.Ignored, A<string>.Ignored, A<EventSubscription>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenDeleteSubscriptionNullSubscriptionNameThrowsException()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeClient, fakeLogger);

            //Act
            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => serviceToTest.DeleteSubscription(null));
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeClient.Subscription_DeleteAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenDeleteSubscriptionDeletesSubscription()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeClient, fakeLogger);

            //Act
            var result = await serviceToTest.DeleteSubscription("a-test-subscription");

            //Assert
            Assert.Equal(HttpStatusCode.OK, result);
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeClient.Subscription_DeleteAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenGetSubscriptionGetsSubscription()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            A.CallTo(() => fakeClient.Subscription_GetByIdAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new EventSubscription("someid", "a-test-subscription", "EventGridTopic"));
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeClient, fakeLogger);

            //Act
            var result = await serviceToTest.GetSubscription("a-test-subscription");

            //Assert
            Assert.Equal("someid", result.Id);
            Assert.Equal("a-test-subscription", result.Name);
            Assert.Equal("EventGridTopic", result.Type);
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeClient.Subscription_GetByIdAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenGetAllSubscriptionsGetsSubscriptions()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            A.CallTo(() => fakeClient.Subscription_GetAllAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new List<EventSubscription> { new EventSubscription("someid", "a-test-subscription-1", "EventGridTopic"), new EventSubscription("someid1", "a-test-subscription-2", "EventGridTopic") });
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeClient, fakeLogger);

            //Act
            var result = await serviceToTest.GetAllSubscriptions();

            //Assert
            Assert.Equal(2, result.Count());
            A.CallTo(() => fakeClient.Subscription_GetAllAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
