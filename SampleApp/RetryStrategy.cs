using System;
using System.Net.Http;

namespace SampleApp
{
    public static class RetryStrategy
    {
        public static bool HttpError(Exception ex)
        {
            return ex.InnerException is HttpRequestException;
        }
    }
}
