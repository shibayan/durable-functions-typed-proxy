using System.Threading.Tasks;

using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableTask.TypedProxy;

/// <summary>
/// Provides the base implementation for the activity proxy.
/// </summary>
/// <typeparam name="TActivityInterface">Activity interface.</typeparam>
public abstract class ActivityProxy<TActivityInterface>
{
    protected ActivityProxy(IDurableOrchestrationContext context)
    {
        _context = context;
    }

    private readonly IDurableOrchestrationContext _context;

    protected internal Task CallAsync(string functionName, object input)
    {
        var retryOptions = RetryOptionsCache.ResolveRetryOptions<TActivityInterface>(functionName);

        if (retryOptions is not null)
        {
            return _context.CallActivityWithRetryAsync(functionName, retryOptions, input);
        }

        return _context.CallActivityAsync(functionName, input);
    }

    protected internal Task<TResult> CallAsync<TResult>(string functionName, object input)
    {
        var retryOptions = RetryOptionsCache.ResolveRetryOptions<TActivityInterface>(functionName);

        if (retryOptions is not null)
        {
            return _context.CallActivityWithRetryAsync<TResult>(functionName, retryOptions, input);
        }

        return _context.CallActivityAsync<TResult>(functionName, input);
    }
}
