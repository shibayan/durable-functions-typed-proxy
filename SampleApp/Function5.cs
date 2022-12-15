using System.Net.Http;
using System.Threading.Tasks;

using DurableTask.TypedProxy;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using SampleApp.Activities;

namespace SampleApp;

public static class Function5
{
    [FunctionName("Function5")]
    public static async Task<string> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var activity = context.CreateActivityProxy<IAliasActivity>();

        return await activity.SayHello("buchizo");
    }

    [FunctionName("Function5_HttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
        [DurableClient] IDurableClient starter,
        ILogger log)
    {
        // Function input comes from the request content.
        string instanceId = await starter.StartNewAsync("Function5", null);

        log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId);
    }
}
