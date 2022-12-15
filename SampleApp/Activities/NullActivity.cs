using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SampleApp.Activities;

public class NullActivity : INullActivity
{
    [FunctionName(nameof(Nop))]
    public Task Nop([ActivityTrigger] object input)
    {
        return Task.CompletedTask;
    }
}
