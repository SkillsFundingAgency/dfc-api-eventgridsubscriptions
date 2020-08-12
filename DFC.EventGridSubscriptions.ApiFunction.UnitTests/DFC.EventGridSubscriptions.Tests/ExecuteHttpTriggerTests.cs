using DFC.Compui.Subscriptions.Pkg.Data;
using DFC.EventGridSubscriptions.ApiFunction.ServiceResult;
using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Services.Interface;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Xunit;

namespace DFC.EventGridSubscriptions.ApiFunction.UnitTests.DFC.EventGridSubscriptions.Tests
{
    public class ExecuteHttpTriggerTests
    {
        private readonly Execute _executeFunction;
        private readonly ILogger _log;
        private readonly HttpRequest _request;
        private readonly ISubscriptionService subscriptionRegistrationService;
        private readonly IOptionsMonitor<AdvancedFilterOptions> advancedFilterOptions;

        public ExecuteHttpTriggerTests()
        {
            _request = A.Fake<HttpRequest>();

            subscriptionRegistrationService = A.Fake<ISubscriptionService>();

            advancedFilterOptions = A.Fake<IOptionsMonitor<AdvancedFilterOptions>>();

            _log = A.Fake<ILogger>();

            _executeFunction = new Execute(subscriptionRegistrationService, advancedFilterOptions);
        }

        [Fact]
        public async Task ExecutePostWhenNoParametersPresentReturnsBadRequestObjectResult()
        {
            //Act
            var result = await RunFunction(null);

            var badRequestObjectResult = result as BadRequestObjectResult;

            // Assert
            Assert.IsAssignableFrom<IActionResult>(result);
            Assert.True(result is BadRequestObjectResult);
            Assert.Equal((int?)HttpStatusCode.BadRequest, badRequestObjectResult.StatusCode);
        }

        [Fact]
        public async Task ExecutePostWhenNullRequestMethodBadRequestObjectResult()
        {
            //Arrange
            A.CallTo(() => _request.Method).Returns("POST");

            //Act
            var result = await RunFunction(null);

            var badRequestObjectResult = result as BadRequestObjectResult;

            // Assert
            Assert.IsAssignableFrom<IActionResult>(result);
            Assert.True(result is BadRequestObjectResult);
            Assert.Equal((int?)HttpStatusCode.BadRequest, badRequestObjectResult.StatusCode);
        }

        [Fact]
        public async Task ExecutePostWhenNullRequestBodyBadRequestObjectResult()
        {
            //Arrange
            A.CallTo(() => _request.Body).Returns(null);

            //Act
            var result = await RunFunction("test-subscription");

            var badRequestObjectResult = result as BadRequestObjectResult;

            // Assert
            Assert.IsAssignableFrom<IActionResult>(result);
            Assert.True(result is BadRequestObjectResult);
            Assert.Equal((int?)HttpStatusCode.BadRequest, badRequestObjectResult.StatusCode);
        }

        [Fact]
        public async Task ExecutePutWhenPutRequestUnprocessableEntityObjectResult()
        {
            //Arrange
            A.CallTo(() => _request.Method).Returns("PUT");

            //Act
            var result = await RunFunction("test-subscription");

            var unprocessableEntityObjectResult = result as UnprocessableEntityObjectResult;

            // Assert
            Assert.IsAssignableFrom<IActionResult>(result);
            Assert.True(result is UnprocessableEntityObjectResult);
            Assert.Equal((int?)HttpStatusCode.UnprocessableEntity, unprocessableEntityObjectResult.StatusCode);
        }

        [Fact]
        public async Task ExecutePostWhenZeroLengthBodyBadRequestObjectResult()
        {
            //Arrange
            A.CallTo(() => _request.Body).Returns(A.Fake<Stream>());

            //Act
            var result = await RunFunction("test-subscription");

            var badRequestObjectResult = result as BadRequestObjectResult;

            // Assert
            Assert.IsAssignableFrom<IActionResult>(result);
            Assert.True(result is BadRequestObjectResult);
            Assert.Equal((int?)HttpStatusCode.BadRequest, badRequestObjectResult.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenAddSubscriptionCalledReturnsCreatedResult()
        {
            //Arrange
            A.CallTo(() => subscriptionRegistrationService.AddSubscription(A<SubscriptionSettings>.Ignored)).Returns(HttpStatusCode.Created);
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, true))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilterValues = 25, MaximumAdvancedFilters = 5 });

            //Act
            CreatedResult result = (CreatedResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenAddSubscriptionCalledReturnsInternalServerErrorResult()
        {
            //Arrange
            A.CallTo(() => subscriptionRegistrationService.AddSubscription(A<SubscriptionSettings>.Ignored)).Returns(HttpStatusCode.InternalServerError);
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, true))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilterValues = 25, MaximumAdvancedFilters = 5 });

            //Act
            InternalServerErrorResult result = (InternalServerErrorResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenDeleteSubscriptionCalledReturnsInternalServerErrorResult()
        {
            //Arrange
            A.CallTo(() => subscriptionRegistrationService.DeleteSubscription(A<string>.Ignored)).Returns(HttpStatusCode.InternalServerError);
            A.CallTo(() => _request.Method).Returns("DELETE");

            //Act
            InternalServerErrorResult result = (InternalServerErrorResult)await RunFunction("test-subscription-name");

            // Assert
            Assert.Equal((int?)HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenAddSubscriptionNoSubscriptionNameCalledReturnsBadRequestObjectResult()
        {
            //Arrange
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, false))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilterValues = 25 });

            //Act
            BadRequestObjectResult result = (BadRequestObjectResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenAddSubscriptionShortSubscriptionNameCalledReturnsBadRequestObjectResult()
        {
            //Arrange
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, true, "te"))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilterValues = 25 });

            //Act
            BadRequestObjectResult result = (BadRequestObjectResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenAddSubscriptionBadSubscriptionNameCalledReturnsBadRequestObjectResult()
        {
            //Arrange
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, true, "#!*something^&*!"))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilterValues = 25 });

            //Act
            BadRequestObjectResult result = (BadRequestObjectResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenAddSubscriptionCalledNoEndpointReturnsBadRequestObjectResult()
        {
            //Arrange
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(false, true, true, true))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilterValues = 25 });

            //Act
            BadRequestObjectResult result = (BadRequestObjectResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenAddSubscriptionCalledNoAdvancedFilterReturnsCreatedObjectResult()
        {
            A.CallTo(() => subscriptionRegistrationService.AddSubscription(A<SubscriptionSettings>.Ignored)).Returns(HttpStatusCode.Created);

            //Arrange
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, false, true))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilterValues = 25 });

            //Act
            CreatedResult result = (CreatedResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenAddSubscriptionCalledAdvancedFiltersExceedMaximumReturnsBadRequestResult()
        {
            //Arrange
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, true, "a-test-subscription", 2))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilters = 1, MaximumAdvancedFilterValues = 25 });

            //Act
            BadRequestObjectResult result = (BadRequestObjectResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenAddSubscriptionCalledAdvancedFilterValuesExceedMaximumReturnsBadRequestResult()
        {
            //Arrange
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, true, "a-test-subscription", 2))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilters = 1 });

            //Act
            BadRequestObjectResult result = (BadRequestObjectResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenAddSubscriptionCalledRelativeEndpointUriReturnsBadRequestResult()
        {
            //Arrange
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, true, "a-test-subscription", 1, "somewhere.com/somelocation", false))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilters = 1 });

            //Act
            BadRequestObjectResult result = (BadRequestObjectResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenDeleteSubscriptionCalledReturnsOkObjectResult()
        {
            //Arrange
            A.CallTo(() => subscriptionRegistrationService.DeleteSubscription(A<string>.Ignored)).Returns(HttpStatusCode.OK);
            A.CallTo(() => _request.Method).Returns("DELETE");

            //Act
            OkObjectResult result = (OkObjectResult)await RunFunction("test-subscription-name");

            // Assert
            Assert.Equal((int?)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenGetSubscriptionCalledReturnsOkObjectResultWithSubscription()
        {
            //Arrange
            A.CallTo(() => subscriptionRegistrationService.GetSubscription(A<string>.Ignored)).Returns(new EventSubscription("testid", "testsubscription", "EventGridSubscription"));
            A.CallTo(() => _request.Method).Returns("GET");

            //Act
            OkObjectResult result = (OkObjectResult)await RunFunction("test-subscription-name");

            var resultAsSubscription = (EventSubscription)result.Value;

            // Assert
            Assert.IsAssignableFrom<EventSubscription>(result.Value);
            Assert.Equal((int?)HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("testid", resultAsSubscription.Id);
            Assert.Equal("testsubscription", resultAsSubscription.Name);
            Assert.Equal("EventGridSubscription", resultAsSubscription.Type);
        }

        [Fact]
        public async Task ExecuteWhenGetSubscriptionsCalledReturnsOkObjectResultWithSubscriptions()
        {
            //Arrange
            A.CallTo(() => subscriptionRegistrationService.GetAllSubscriptions()).Returns(new List<EventSubscription>() { new EventSubscription("testid", "testsubscription", "EventGridSubscription"), new EventSubscription("testid-2", "testsubscription-2", "EventGridSubscription") });
            A.CallTo(() => _request.Method).Returns("GET");

            //Act
            OkObjectResult result = (OkObjectResult)await RunFunction(string.Empty);

            var resultAsSubscription = (List<EventSubscription>)result.Value;

            // Assert
            Assert.IsAssignableFrom<List<EventSubscription>>(result.Value);
            Assert.Equal((int?)HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(2, resultAsSubscription.Count);
        }

        [Fact]
        public async Task ExecuteWhenDeleteSubscriptionCalledReturnsGenericInternalServerErrorResult()
        {
            //Arrange
            A.CallTo(() => subscriptionRegistrationService.DeleteSubscription(A<string>.Ignored)).Throws<ArithmeticException>();
            A.CallTo(() => _request.Method).Returns("DELETE");

            //Act
            InternalServerErrorResult result = (InternalServerErrorResult)await RunFunction("test-subscription-name");

            // Assert
            Assert.Equal((int?)HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenDeleteSubscriptionCalledReturnsServiceUnavailableResult()
        {
            //Arrange
            A.CallTo(() => subscriptionRegistrationService.DeleteSubscription(A<string>.Ignored)).Throws<RestException>();
            A.CallTo(() => _request.Method).Returns("DELETE");
            //Execute async manually - happens automatically in request pipeline
            var context = new ActionContext();
            context.HttpContext = A.Fake<HttpContext>();

            //Act
            ServiceUnavailableObjectResult result = (ServiceUnavailableObjectResult)await RunFunction("test-subscription-name");
            await result.ExecuteResultAsync(context);

            // Assert
            Assert.Equal((int?)HttpStatusCode.ServiceUnavailable, context.HttpContext.Response.StatusCode);
            Assert.Equal((int?)HttpStatusCode.ServiceUnavailable, (int)result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenDeleteSubscriptionCalledReturnsNullActionContextParameter()
        {
            //Arrange
            A.CallTo(() => subscriptionRegistrationService.DeleteSubscription(A<string>.Ignored)).Throws<RestException>();
            A.CallTo(() => _request.Method).Returns("DELETE");
            //Execute async manually - happens automatically in request pipeline
            var context = new ActionContext();
            context.HttpContext = A.Fake<HttpContext>();

            //Act
            ServiceUnavailableObjectResult result = (ServiceUnavailableObjectResult)await RunFunction("test-subscription-name");

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await result.ExecuteResultAsync(null).ConfigureAwait(false));
        }

        [Fact]
        public async Task ExecuteWhenDeleteSubscriptionCalledNullSubscriptionNameReturnsBadRequestObjectResult()
        {
            //Arrange
            A.CallTo(() => subscriptionRegistrationService.DeleteSubscription(A<string>.Ignored)).Returns(HttpStatusCode.OK);
            A.CallTo(() => _request.Method).Returns("DELETE");

            //Act
            BadRequestObjectResult result = (BadRequestObjectResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.BadRequest, result.StatusCode);
        }

        private async Task<IActionResult> RunFunction(string subscriptionName)
        {
            return await _executeFunction.Run(_request, _log, subscriptionName).ConfigureAwait(false);
        }

        private string GetRequestBody(bool includeEndpoint, bool includeSimpleFilter, bool includeAdvancedFilter, bool includeName, string subscriptionName = "A-Test-Subscription", int numberOfFilters = 1, string endpointAddress = "http://somewhere.com/somewebhook/receive", bool isUriAbsolute = true)
        {
            var advancedFilters = new List<SubscriptionPropertyContainsFilter>();

            for (int i = 0; i < numberOfFilters; i++)
            {
                advancedFilters.Add(new SubscriptionPropertyContainsFilter { Key = "subject", Values = new List<string> { "a", "b", "c" }.ToArray() });
            }

            return JsonConvert.SerializeObject(new SubscriptionSettings
            {
                Endpoint = includeEndpoint ? new Uri(endpointAddress, isUriAbsolute ? UriKind.Absolute : UriKind.Relative) : null,
                Filter = new SubscriptionFilter { BeginsWith = includeSimpleFilter ? "abeginswith" : null, EndsWith = includeSimpleFilter ? "anendswith" : null, PropertyContainsFilters = includeAdvancedFilter ? advancedFilters : null },
                Name = includeName ? subscriptionName : null
            });
        }
    }
}
