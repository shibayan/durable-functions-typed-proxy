using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;

namespace SampleApp.Activities
{
    public class AliasActivity : IAliasActivity
    {
        [FunctionName("Say")]
        public Task<string> SayHello([ActivityTrigger] string name)
        {
            return Task.FromResult($"Hello {name}.");
        }
    }
}
