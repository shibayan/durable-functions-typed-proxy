using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using FunctionApp67.Activities;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp67
{
    public class Function1
    {
        [FunctionName("Function1")]
        public async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var proxy = context.CreateActivityProxy<IHelloActivity>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await proxy.SayHello("Tokyo"));
            outputs.Add(await proxy.SayHello("Seattle"));
            outputs.Add(await proxy.SayHello("London"));

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

            return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId);
        }
    }
}