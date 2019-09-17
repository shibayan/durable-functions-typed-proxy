using System.Collections.Concurrent;
using System.Reflection;

using Microsoft.Azure.WebJobs;

namespace DurableTask.ActivityProxy
{
    internal static class RetryOptionsCache
    {
        private static readonly ConcurrentDictionary<string, RetryOptionsAttribute> RetryOptions = new ConcurrentDictionary<string, RetryOptionsAttribute>();

        internal static RetryOptions ResolveRetryOptions<TActivityInterface>(string functionName)
        {
            var attribute = RetryOptions.GetOrAdd(functionName, x => typeof(TActivityInterface).GetMethod(x)
                                                                                               ?.GetCustomAttribute<RetryOptionsAttribute>(true));

            return attribute?.ToRetryOptions();
        }
    }
}
