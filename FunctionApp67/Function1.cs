using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp67
{
    public class Function1
    {
        [FunctionName("Function1")]
        public async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            var outputs = new List<string>();

            var proxy = context.CreateActivityProxy<ISampleActivity>();

            var (v1, v2) = await proxy.TupleTest((50, "kazuakix"));

            log.LogWarning(v1);
            log.LogWarning(v2);

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await proxy.SayHello("Tokyo"));
            outputs.Add(await proxy.SayHello("Seattle"));
            outputs.Add(await proxy.SayHello("London"));

            // ブチザッキのタイトルを取る
            var content = await proxy.HttpGet("https://blog.azure.moe/");

            var match = Regex.Match(content, @"<title>(.+?)<\/title>");

            if (match.Success)
            {
                outputs.Add(match.Groups[1].Value);
            }

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("Function1_HttpStart")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function1", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}