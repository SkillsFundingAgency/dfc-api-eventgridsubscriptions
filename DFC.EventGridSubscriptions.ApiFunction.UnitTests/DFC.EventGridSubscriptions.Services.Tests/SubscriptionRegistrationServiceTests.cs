using Castle.Core.Logging;
using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DFC.EventGridSubscriptions.ApiFunction.UnitTests.DFC.EventGridSubscriptions.Services.Tests
{
    public class SubscriptionRegistrationServiceTests
    {
        private readonly IOptionsMonitor<EventGridSubscriptionClientOptions> fakeClientOptions = A.Fake<IOptionsMonitor<EventGridSubscriptionClientOptions>>();
        private readonly ILogger<SubscriptionRegistrationService> fakeLogger = A.Fake<ILogger<SubscriptionRegistrationService>>();

        [Fact]
        public void DoSomething()
        {
            //Arrange


            //Act
            var serviceToTest = new SubscriptionRegistrationService(fakeClientOptions, fakeLogger);
            //Assert
        }
    }
}
