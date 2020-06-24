using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("Execute")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "delete", Route = "Execute/{subscriptionName}")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Here");

            if (req == null || !req.Headers.Any())
            {
                throw new ArgumentNullException(nameof(req));
            }

            switch (req.Method.ToUpperInvariant())
            {
                case "PSOT":
                    var addResult = await subscriptionRegistrationService.AddSubscription().ConfigureAwait(false);
                    return new StatusCodeResult(201);
                case "DELETE":
                    var deleteResult = await subscriptionRegistrationService.DeleteSubcription().ConfigureAwait(false);
                    return new StatusCodeResult(200);
                default:
                    return new StatusCodeResult(404);
            }
        }
    }
}
