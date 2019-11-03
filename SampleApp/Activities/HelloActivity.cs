using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SampleApp.Activities
{
    public class HelloActivity : IHelloActivity
    {
        [FunctionName(nameof(SayHello))]
        public Task<string> SayHello([ActivityTrigger] string name)
        {
            return Task.FromResult($"Hello {name}!");
        }
    }
}