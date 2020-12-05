using System.Net.Http;
using System.Threading.Tasks;

using DurableTask.TypedProxy;

namespace SampleApp.Activities
{
    public interface IHttpGetActivity
    {
        [RetryOptions("00:00:05", 5, HandlerType = typeof(ExceptionRetryStrategy<HttpRequestException>))]
        Task<string> HttpGet(string path);
    }
}
