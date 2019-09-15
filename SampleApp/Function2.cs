using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using SampleApp.Activities;

namespace SampleApp
{
    public class Function2
    {
        [FunctionName("Function2")]
        public async Task<string> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var activity = context.CreateActivityProxy<IHttpGetActivity>();

            // ブチザッキのタイトルを取る
            var content = await activity.HttpGet("https://blog.azure.moe/");

            var match = Regex.Match(content, @"<title>(.+?)<\/title>");

            return match.Success ? match.Groups[1].Value : "";
        }

        [FunctionName("Function2_HttpStart")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function2", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId);
        }
    }
}