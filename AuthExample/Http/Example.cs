using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication;

namespace AuthExample.Http
{
    public static class Example
    {
        [FunctionName(nameof(Example))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var result = await req.HttpContext.AuthenticateAsync("Test");
            if (!result.Succeeded)
            {
                return new UnauthorizedResult();
            }
            else
            {
                return new OkObjectResult("This HTTP triggered function executed successfully.");
            }
        }
    }
}
