using System.Collections.Concurrent;
using System.Reflection;

using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableTask.TypedProxy;

internal static class RetryOptionsCache
{
    private static readonly ConcurrentDictionary<string, RetryOptionsAttribute> s_retryOptions = new();

    internal static RetryOptions ResolveRetryOptions<TActivityInterface>(string functionName)
    {
        var attribute = s_retryOptions.GetOrAdd(functionName, x => typeof(TActivityInterface).GetMethod(x)
                                                                                             ?.GetCustomAttribute<RetryOptionsAttribute>(true));

        return attribute?.ToRetryOptions();
    }
}
