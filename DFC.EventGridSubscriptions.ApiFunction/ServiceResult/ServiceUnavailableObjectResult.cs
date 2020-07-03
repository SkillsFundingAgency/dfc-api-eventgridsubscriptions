using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.ApiFunction.ServiceResult
{
    public class ServiceUnavailableObjectResult : IActionResult
    {
        private readonly string message;

        public ServiceUnavailableObjectResult(string message)
        {
            this.message = message;
            this.StatusCode = HttpStatusCode.ServiceUnavailable;
        }

        public HttpStatusCode StatusCode { get; private set; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            ValidateParameters(context);

            context.HttpContext.Response.StatusCode = 503;

            var myByteArray = Encoding.UTF8.GetBytes(message);
            await context.HttpContext.Response.Body.WriteAsync(myByteArray, 0, myByteArray.Length).ConfigureAwait(false);
            await context.HttpContext.Response.Body.FlushAsync().ConfigureAwait(false);
        }

        private static void ValidateParameters(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
        }
    }
}
