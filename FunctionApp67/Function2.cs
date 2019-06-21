using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp67
{
    public static class Function2
    {
        [FunctionName("Function2")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var proxy = context.CreateActivityProxy<ISampleActivity>();

            // ブチザッキのタイトルを取る
            var content = await proxy.HttpGet("https://blog.azure.moe/");

            var match = Regex.Match(content, @"<title>(.+?)<\/title>");

            return match.Success ? match.Groups[1].Value : "";
        }

        [FunctionName("Function2_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function2", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}