using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableTask.TypedProxy;

/// <summary>
/// Defines convenient overloads for creating activity proxy.
/// </summary>
public static class ActivityProxyExtensions
{
    /// <summary>
    /// Create a new type-safe activity proxy instance.
    /// </summary>
    /// <typeparam name="TActivityInterface">Activity interface.</typeparam>
    /// <param name="context">Current orchestration context.</param>
    /// <returns>New activity proxy instance.</returns>
    public static TActivityInterface CreateActivityProxy<TActivityInterface>(this IDurableOrchestrationContext context) => ActivityProxyFactory.Create<TActivityInterface>(context);
}
