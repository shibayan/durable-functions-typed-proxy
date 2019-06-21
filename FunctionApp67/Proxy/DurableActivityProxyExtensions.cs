using Microsoft.Azure.WebJobs;

namespace FunctionApp67
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class DurableActivityProxyExtensions
    {
        public static TActivityInterface CreateActivityProxy<TActivityInterface>(this DurableOrchestrationContext context)
        {
            return ActivityProxyFactory.Create<TActivityInterface>(context);
        }
    }
}