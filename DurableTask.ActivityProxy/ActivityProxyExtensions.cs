namespace Microsoft.Azure.WebJobs
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ActivityProxyExtensions
    {
        public static TActivityInterface CreateActivityProxy<TActivityInterface>(this DurableOrchestrationContext context)
        {
            return ActivityProxyFactory.Create<TActivityInterface>(context);
        }
    }
}