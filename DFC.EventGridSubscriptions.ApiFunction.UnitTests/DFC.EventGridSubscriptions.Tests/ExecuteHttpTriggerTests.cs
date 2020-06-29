using DFC.EventGridSubscriptions.Data;
using DFC.EventGridSubscriptions.Data.Models;
using DFC.EventGridSubscriptions.Services.Interface;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly ISubscriptionRegistrationService subscriptionRegistrationService;
        private readonly IOptionsMonitor<AdvancedFilterOptions> advancedFilterOptions;

        public ExecuteHttpTriggerTests()
        {
            _request = A.Fake<HttpRequest>();

            subscriptionRegistrationService = A.Fake<ISubscriptionRegistrationService>();

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
            A.CallTo(() => subscriptionRegistrationService.AddSubscription(A<SubscriptionRequest>.Ignored)).Returns(HttpStatusCode.Created);
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, true))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilterValues = 25 });

            //Act
            CreatedResult result = (CreatedResult)await RunFunction(null);

            // Assert
            Assert.Equal((int?)HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteWhenAddSubscriptionCalledReturnsInternalServerErrorResult()
        {
            //Arrange
            A.CallTo(() => subscriptionRegistrationService.AddSubscription(A<SubscriptionRequest>.Ignored)).Returns(HttpStatusCode.InternalServerError);
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, true))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilterValues = 25 });

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
            A.CallTo(() => subscriptionRegistrationService.AddSubscription(A<SubscriptionRequest>.Ignored)).Returns(HttpStatusCode.Created);

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
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, true))));
            A.CallTo(() => advancedFilterOptions.CurrentValue).Returns(new AdvancedFilterOptions { MaximumAdvancedFilterValues = 1 });

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

        private string GetRequestBody(bool includeEndpoint, bool includeSimpleFilter, bool includeAdvancedFilter, bool includeName)
        {
            return JsonConvert.SerializeObject(new SubscriptionRequest
            {
                Endpoint = includeEndpoint ? new Uri("http://somewhere.com/somewebhook/receive") : null,
                Filter = new SubscriptionFilter { BeginsWith = includeSimpleFilter ? "abeginswith" : null, EndsWith = includeSimpleFilter ? "anendswith"  : null, PropertyContainsFilter = includeAdvancedFilter ? new StringInAdvancedFilter("subject", new List<string> { "a", "b", "c" }) : null },
                Name = includeName ? "A-Test-Subscription" : null
            });
        }
    }
}
