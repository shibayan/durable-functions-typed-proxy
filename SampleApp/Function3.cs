using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DurableTask.TypedProxy;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using SampleApp.Activities;

namespace SampleApp
{
    public class Function3
    {
        [FunctionName("Function3")]
        public async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var activity = context.CreateActivityProxy<IHelloActivity>();

            var input = new[] { "Tokyo", "Seattle", "London" };

            var tasks = new Task<string>[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                tasks[i] = activity.SayHello(input[i]);
            }

            await Task.WhenAll(tasks);

            return tasks.Select(x => x.Result).ToList();
        }

        [FunctionName("Function3_HttpStart")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function3", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId);
        }
    }
}