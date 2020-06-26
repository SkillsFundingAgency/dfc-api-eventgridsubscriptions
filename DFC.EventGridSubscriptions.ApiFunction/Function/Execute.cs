using DFC.EventGridSubscriptions.Data.Models;
using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.ApiFunction
{
    /// <summary>
    /// The Execute Function.
    /// </summary>
    public class Execute
    {
        private readonly ISubscriptionRegistrationService subscriptionRegistrationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Execute"/> class.
        /// </summary>
        /// <param name="subscriptionRegistrationService">A Subscription Registration Service.</param>
        public Execute(ISubscriptionRegistrationService subscriptionRegistrationService)
        {
            this.subscriptionRegistrationService = subscriptionRegistrationService;
        }

        /// <summary>
        /// Runs the function.
        /// </summary>
        /// <param name="req">The Request.</param>
        /// <param name="log">The Logger.</param>
        /// <param name="subscriptionName">The subscription name.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("Execute")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "delete", Route = "Execute/{subscriptionName?}")] HttpRequest req, ILogger log, string subscriptionName)
        {
            log.LogInformation("Subscription function execution started");

            if (req == null || req.Body == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            switch (req.Method.ToUpperInvariant())
            {
                case "POST":
                    return await HandlePostAsync(req, log).ConfigureAwait(false);
                case "DELETE":
                    return await HandleDeleteAsync(log, subscriptionName).ConfigureAwait(false);
                default:
                    return new StatusCodeResult(404);
            }
        }

        private static bool ValidateBodyParameters(SubscriptionRequest request, out string message)
        {
            message = string.Empty;

            if (string.IsNullOrEmpty(request.Name))
            {
                message = $"{nameof(request.Name)} not present in request";
                return false;
            }

            if (request.Endpoint == null)
            {
                message = $"{nameof(request.Endpoint)} not present in request";
                return false;
            }

            //No more than 5 advanced filters are supported by Event Grid
            if(request.Filter != null && request.Filter.AdvancedFilters != null && request.Filter.AdvancedFilters.Count > 5)
            {
                message = $"{nameof(request.Filter.AdvancedFilters)} cannot provide more than 5 advanced filters";
            }

            return true;
        }

        private static async Task<SubscriptionRequest> GetBodyParametersAsync(Stream body)
        {
            using (var stream = new StreamReader(body))
            {
                var content = await stream.ReadToEndAsync().ConfigureAwait(false);

                //Extract Request Body and Parse To Class
                SubscriptionRequest subscriptionRequest = JsonConvert.DeserializeObject<SubscriptionRequest>(content);

                return subscriptionRequest;
            }
        }

        private async Task<IActionResult> HandleDeleteAsync(ILogger log, string subscriptionName)
        {
            log.LogInformation("Function Deleting Subscription");

            if (string.IsNullOrWhiteSpace(subscriptionName))
            {
                return new BadRequestObjectResult($"{nameof(subscriptionName)} not present in request");
            }

            var deleteResult = await subscriptionRegistrationService.DeleteSubscription(subscriptionName).ConfigureAwait(false);

            return new ContentResult { StatusCode = (int)deleteResult };
        }

        private async Task<IActionResult> HandlePostAsync(HttpRequest req, ILogger log)
        {
            log.LogInformation("Function Creating Subscription");

            var bodyParameters = await GetBodyParametersAsync(req.Body).ConfigureAwait(false);

            var validBodyParameters = ValidateBodyParameters(bodyParameters, out string message);
            if (!validBodyParameters)
            {
                return new BadRequestObjectResult(message);
            }

            var addResult = await subscriptionRegistrationService.AddSubscription(bodyParameters).ConfigureAwait(false);
            return new StatusCodeResult((int)addResult);
        }
    }
}
