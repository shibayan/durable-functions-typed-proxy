using System;
using System.Collections.Concurrent;
using System.Reflection;

using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableTask.TypedProxy
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RetryOptionsAttribute : Attribute
    {
        public RetryOptionsAttribute(string firstRetryInterval, int maxNumberOfAttempts)
        {
            if (maxNumberOfAttempts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxNumberOfAttempts));
            }

            FirstRetryInterval = firstRetryInterval ?? throw new ArgumentNullException(nameof(firstRetryInterval));
            MaxNumberOfAttempts = maxNumberOfAttempts;
        }

        public string FirstRetryInterval { get; }
        public string MaxRetryInterval { get; set; }
        public double? BackoffCoefficient { get; set; }
        public string RetryTimeout { get; set; }
        public int MaxNumberOfAttempts { get; }
        public Type HandlerType { get; set; }
        public string HandlerMethodName { get; set; } = "Handle";

        internal RetryOptions ToRetryOptions()
        {
            var retryOptions = new RetryOptions(TimeSpan.Parse(FirstRetryInterval), MaxNumberOfAttempts);

            if (!string.IsNullOrEmpty(MaxRetryInterval))
            {
                retryOptions.MaxRetryInterval = TimeSpan.Parse(MaxRetryInterval);
            }

            if (BackoffCoefficient.HasValue)
            {
                retryOptions.BackoffCoefficient = BackoffCoefficient.Value;
            }

            if (!string.IsNullOrEmpty(RetryTimeout))
            {
                retryOptions.RetryTimeout = TimeSpan.Parse(RetryTimeout);
            }

            if (HandlerType != null && !string.IsNullOrEmpty(HandlerMethodName))
            {
                retryOptions.Handle = HandlerCache.GetOrAdd((HandlerType, HandlerMethodName), CreateDelegate);
            }

            return retryOptions;
        }

        private static readonly ConcurrentDictionary<(Type, string), Func<Exception, bool>> HandlerCache = new ConcurrentDictionary<(Type, string), Func<Exception, bool>>();

        private static Func<Exception, bool> CreateDelegate((Type handlerType, string methodName) input)
        {
            var (handlerType, methodName) = input;

            var methodInfo = handlerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            if (methodInfo == null)
            {
                throw new InvalidOperationException($"{handlerType.FullName}.{methodName} static method not found.");
            }

            return (Func<Exception, bool>)Delegate.CreateDelegate(typeof(Func<Exception, bool>), methodInfo);
        }
    }
}
