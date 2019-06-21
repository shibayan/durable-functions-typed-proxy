using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;

namespace SampleApp.Activities
{
    public interface IHttpGetActivity
    {
        [RetryOptions("00:00:05", 10)]
        Task<string> HttpGet(string path);
    }
}