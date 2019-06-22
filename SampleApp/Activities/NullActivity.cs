using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;

namespace SampleApp.Activities
{
    public class NullActivity : INullActivity
    {
        [FunctionName(nameof(Nop))]
        public Task Nop([ActivityTrigger] object input)
        {
            return Task.CompletedTask;
        }
    }
}
