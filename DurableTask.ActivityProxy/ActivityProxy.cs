using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs
{
    public abstract class ActivityProxy<TActivityInterface>
    {
        protected ActivityProxy(DurableOrchestrationContext context)
        {
            _context = context;
        }

        private readonly DurableOrchestrationContext _context;

        protected internal Task CallAsync(string functionName, object input)
        {
            var retryOptions = ResolveRetryOptions(functionName);

            if (retryOptions != null)
            {
                return _context.CallActivityWithRetryAsync(functionName, retryOptions, input);
            }

            return _context.CallActivityAsync(functionName, input);
        }

        protected internal Task<TResult> CallAsync<TResult>(string functionName, object input)
        {
            var retryOptions = ResolveRetryOptions(functionName);

            if (retryOptions != null)
            {
                return _context.CallActivityWithRetryAsync<TResult>(functionName, retryOptions, input);
            }

            return _context.CallActivityAsync<TResult>(functionName, input);
        }

        private static readonly ConcurrentDictionary<string, RetryOptionsAttribute> RetryOptionsCache = new ConcurrentDictionary<string, RetryOptionsAttribute>();

        private static RetryOptions ResolveRetryOptions(string functionName)
        {
            var attribute = RetryOptionsCache.GetOrAdd(functionName, x => typeof(TActivityInterface).GetMethod(x)
                                                                                                    ?.GetCustomAttribute<RetryOptionsAttribute>(true));

            return attribute?.ToRetryOptions();
        }
    }
}