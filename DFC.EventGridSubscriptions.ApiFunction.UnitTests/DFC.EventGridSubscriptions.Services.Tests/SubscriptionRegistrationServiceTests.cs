using DFC.Compui.Cosmos.Contracts;
using DFC.Compui.Subscriptions.Pkg.Data;
using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services;
using FakeItEasy;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Management.EventGrid.Models;
using Microsoft.Azure.Management.ResourceManager.Models;
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
        private readonly ILogger<SubscriptionService> fakeLogger = A.Fake<ILogger<SubscriptionService>>();
        private readonly IEventGridManagementClientWrapper fakeClient = A.Fake<IEventGridManagementClientWrapper>();
        private readonly IDocumentService<SubscriptionModel> fakeDocumentClient = A.Fake<IDocumentService<SubscriptionModel>>();

        public SubscriptionRegistrationServiceTests()
        {
            A.CallTo(() => fakeClientOptions.CurrentValue).Returns(new EventGridSubscriptionClientOptions { DeadLetterBlobContainerName = "DeadLetterContainer", StaleSubsriptionInterval = new TimeSpan(0, 0, 0, 10), StaleSubsriptionThreshold = 2 });
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenAddSubscriptionAddsSubscription()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, A.Fake<IDocumentService<SubscriptionModel>>(), fakeLogger);

            //Act
            var result = await serviceToTest.AddSubscription(new SubscriptionSettings { Endpoint = new Uri("http://somehost.com/awebhook"), Name = "Test Subscriber" });

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
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, A.Fake<IDocumentService<SubscriptionModel>>(), fakeLogger);

            //Act
            var result = await serviceToTest.AddSubscription(new SubscriptionSettings { Endpoint = new Uri("http://somehost.com/awebhook"), Name = "Test Subscriber", Filter = new SubscriptionFilter { PropertyContainsFilters = new List<SubscriptionPropertyContainsFilter> { new SubscriptionPropertyContainsFilter() { Key = "subject", Values = new List<string> { "a", "b", "c", "d", "e" }.ToArray() } } } });

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
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, A.Fake<IDocumentService<SubscriptionModel>>(), fakeLogger);

            //Act
            //Assert
            await Assert.ThrowsAsync<ArgumentException>(() => serviceToTest.AddSubscription(new SubscriptionSettings { Endpoint = new Uri("http://somehost.com/awebhook"), Name = null, Filter = new SubscriptionFilter { PropertyContainsFilters = new List<SubscriptionPropertyContainsFilter> { new SubscriptionPropertyContainsFilter { Key = "subject", Values = new List<string> { "1", "2", "3", "4", "5" }.ToArray() } } } }));


            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeClient.Subscription_CreateOrUpdateAsync(A<string>.Ignored, A<string>.Ignored, A<EventSubscription>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenAddSubscriptionNullNameThrowsException()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, A.Fake<IDocumentService<SubscriptionModel>>(), fakeLogger);

            //Act
            //Assert
            await Assert.ThrowsAsync<ArgumentException>(() => serviceToTest.AddSubscription(new SubscriptionSettings { Endpoint = new Uri("http://somehost.com/awebhook"), Name = null }));
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeClient.Subscription_CreateOrUpdateAsync(A<string>.Ignored, A<string>.Ignored, A<EventSubscription>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenAddSubscriptionNullEndpointThrowsException()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, A.Fake<IDocumentService<SubscriptionModel>>(), fakeLogger);

            //Act
            //Assert
            await Assert.ThrowsAsync<ArgumentException>(() => serviceToTest.AddSubscription(new SubscriptionSettings { Endpoint = null, Name = "a-test-subscription" }));
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeClient.Subscription_CreateOrUpdateAsync(A<string>.Ignored, A<string>.Ignored, A<EventSubscription>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenAddSubscriptionNullRequestThrowsException()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, A.Fake<IDocumentService<SubscriptionModel>>(), fakeLogger);

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
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, A.Fake<IDocumentService<SubscriptionModel>>(), fakeLogger);

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
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, A.Fake<IDocumentService<SubscriptionModel>>(), fakeLogger);

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
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, A.Fake<IDocumentService<SubscriptionModel>>(), fakeLogger);

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
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, A.Fake<IDocumentService<SubscriptionModel>>(), fakeLogger);

            //Act
            var result = await serviceToTest.GetAllSubscriptions();

            //Assert
            Assert.Equal(2, result.Count());
            A.CallTo(() => fakeClient.Subscription_GetAllAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenFirstStaleSubscriptionUpdatesSubscription()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            A.CallTo(() => fakeClient.Subscription_GetByIdAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new EventSubscription("someid", "a-test-subscription", "EventGridTopic"));
            A.CallTo(() => fakeDocumentClient.UpsertAsync(A<SubscriptionModel>.Ignored)).Returns(HttpStatusCode.OK);
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, fakeDocumentClient, fakeLogger);

            //Act
            var result = await serviceToTest.StaleSubscription("a-test-subscription");

            //Assert
            A.CallTo(() => fakeDocumentClient.UpsertAsync(A<SubscriptionModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeClient.Subscription_DeleteAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            Assert.Equal(200, (int)result);
        }

        [Fact]
        public async Task SubscriptionRegistrationServiceWhenStaleSubscriptionExceedsThresholdUpdatesAndDeletesSubscription()
        {
            //Arrange
            A.CallTo(() => fakeClient.Topic_GetAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new Topic("location", "someid", "sometopic"));
            A.CallTo(() => fakeClient.Subscription_GetByIdAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(new EventSubscription("someid", "a-test-subscription", "EventGridTopic"));
            A.CallTo(() => fakeDocumentClient.UpsertAsync(A<SubscriptionModel>.Ignored)).Returns(HttpStatusCode.OK);
            A.CallTo(() => fakeClientOptions.CurrentValue).Returns(new EventGridSubscriptionClientOptions { StaleSubsriptionThreshold = 1, StaleSubsriptionInterval = new TimeSpan(0, 0, 10) });
            var serviceToTest = new SubscriptionService(fakeClientOptions, fakeClient, fakeDocumentClient, fakeLogger);

            //Act
            var result = await serviceToTest.StaleSubscription("a-test-subscription");

            //Assert
            A.CallTo(() => fakeDocumentClient.UpsertAsync(A<SubscriptionModel>.Ignored)).MustHaveHappenedTwiceExactly();
            A.CallTo(() => fakeClient.Subscription_DeleteAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(200, (int)result);
        }
    }
}
