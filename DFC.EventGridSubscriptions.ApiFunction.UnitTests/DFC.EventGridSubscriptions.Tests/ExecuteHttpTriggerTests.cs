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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFC.EventGridSubscriptions.ApiFunction.UnitTests.DFC.EventGridSubscriptions.Tests
{
    public class ExecuteHttpTriggerTests
    {
        private readonly Execute _executeFunction;
        private readonly ILogger _log;
        private readonly HttpRequest _request;
        private readonly IOptionsMonitor<EventGridSubscriptionClientOptions> _EventGridSubscriptionClientOptions;
        private readonly ISubscriptionRegistrationService _SubscriptionRegistrationService;

        public ExecuteHttpTriggerTests()
        {
            _request = A.Fake<HttpRequest>();

            _SubscriptionRegistrationService = A.Fake<ISubscriptionRegistrationService>();

            _log = A.Fake<ILogger>();

            _executeFunction = new Execute(_SubscriptionRegistrationService, A.Fake<IOptionsMonitor<AdvancedFilterOptions>>());
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
            A.CallTo(() => _request.Method).Returns("test-subscription");

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
        public async Task ExecuteWhenAddSubscriptionCalledReturns200StatusCode()
        {
            //Arrange
            A.CallTo(() => _SubscriptionRegistrationService.AddSubscription(A<SubscriptionRequest>.Ignored)).Returns(HttpStatusCode.Created);
            A.CallTo(() => _request.Method).Returns("POST");
            A.CallTo(() => _request.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(GetRequestBody(true, true, true, true))));

            //Act
            StatusCodeResult result = (StatusCodeResult)await RunFunction("test-subscription-name");

            // Assert
            Assert.IsAssignableFrom<IActionResult>(result);
            Assert.Equal((int?)HttpStatusCode.Created, result.StatusCode);
        }

        //[Fact]
        //public async Task Execute_GetAllPages_ReturnsCorrectJsonResponse()
        //{
        //    var recordJsonInput = File.ReadAllText(Directory.GetCurrentDirectory() + "/Files/Input/PageRecordInput_GetAll.json");
        //    var expectedJsonOutput = File.ReadAllText(Directory.GetCurrentDirectory() + "/Files/Output/PageRecordOutput_GetAll.json");

        //    A.CallTo(() => _graphDatabase.Run(A<GenericCypherQuery>.Ignored, A<string>.Ignored)).Returns(new List<IRecord>() { new Api.Content.UnitTests.Models.Record(new string[] { "data.properties" }, new object[] { JsonConvert.DeserializeObject<Dictionary<string, object>>(recordJsonInput.ToString()) }) });

        //    var result = await RunFunction("test1", null);
        //    var okObjectResult = result as OkObjectResult;

        //    // Assert
        //    Assert.True(result is OkObjectResult);

        //    var resultJson = JsonConvert.SerializeObject(okObjectResult.Value);

        //    var equal = JToken.DeepEquals(JToken.Parse(expectedJsonOutput), JToken.Parse(resultJson));
        //    Assert.True(equal);
        //}

        //[Fact]
        //public async Task Execute_GetPage_ReturnsCorrectJsonResponse()
        //{
        //    var recordJsonInput = File.ReadAllText(Directory.GetCurrentDirectory() + "/Files/Input/PageRecordInput_GetById.json");
        //    var expectedJsonOutput = File.ReadAllText(Directory.GetCurrentDirectory() + "/Files/Output/PageRecordOutput_GetById.json");

        //    var driverRecords = new List<IRecord>() { new Api.Content.UnitTests.Models.Record(new string[] { "values" }, new object[] { JsonConvert.DeserializeObject<Dictionary<string, object>>(recordJsonInput.ToString()) }) };

        //    A.CallTo(() => _graphDatabase.Run(A<GenericCypherQuery>.Ignored, A<string>.Ignored)).Returns(driverRecords);

        //    var result = await RunFunction("test1", Guid.NewGuid());
        //    var okObjectResult = result as OkObjectResult;

        //    // Assert
        //    Assert.True(result is OkObjectResult);

        //    var equal = JToken.DeepEquals(JToken.Parse(okObjectResult.Value.ToString()), JToken.Parse(expectedJsonOutput));
        //    Assert.True(equal);
        //}

        private async Task<IActionResult> RunFunction(string subscriptionName)
        {
            return await _executeFunction.Run(_request, _log, subscriptionName).ConfigureAwait(false);
        }

        private string GetRequestBody(bool includeEndpoint, bool includeSimpleFilter, bool insludeAdvancedFilter, bool includeName)
        {
            return JsonConvert.SerializeObject(new SubscriptionRequest
            {
                Endpoint = new Uri("http://somewhere.com/somewebhook/receive"),
                Filter = new SubscriptionFilter { BeginsWith = "abeginswith", EndsWith = "anendswith", SubjectContainsFilter = new StringInAdvancedFilter("subject", new List<string> { "a", "b", "c" }) },
            Name = "A-Test-Subscription"
            });
        }
    }
}
