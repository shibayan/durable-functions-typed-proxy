using System;

using Microsoft.Azure.WebJobs;

namespace FunctionApp67
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RetryOptionsAttribute : Attribute
    {
        public RetryOptionsAttribute(string firstRetryInterval, int maxNumberOfAttempts)
        {
            FirstRetryInterval = firstRetryInterval;
            MaxNumberOfAttempts = maxNumberOfAttempts;
        }

        public string FirstRetryInterval { get; }
        public string MaxRetryInterval { get; set; }
        public double? BackoffCoefficient { get; set; }
        public string RetryTimeout { get; set; }
        public int MaxNumberOfAttempts { get; }

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

            return retryOptions;
        }
    }
}