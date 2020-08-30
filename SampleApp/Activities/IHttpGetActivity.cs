using System.Threading.Tasks;

using DurableTask.TypedProxy;

using Microsoft.Azure.WebJobs;

namespace SampleApp.Activities
{
    public interface IHttpGetActivity
    {
        [RetryOptions("00:00:05", 5, HandlerType = typeof(RetryStrategy), HandlerMethodName = nameof(RetryStrategy.HttpError))]
        Task<string> HttpGet(string path);
    }
}
