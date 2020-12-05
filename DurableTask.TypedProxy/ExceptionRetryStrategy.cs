using System;

namespace DurableTask.TypedProxy
{
    public static class ExceptionRetryStrategy<TException>
    {
        public static bool Handle(Exception exception)
        {
            return exception.InnerException is TException;
        }
    }
}
