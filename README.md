# Type-safe activity helper for Durable Functions

## NuGet Packages

Package Name | Target Framework | NuGet
---|---|---
DurableTask.ActivityProxy | .NET Standard 2.0 | [![NuGet](https://img.shields.io/nuget/v/DurableTask.ActivityProxy.svg)](https://www.nuget.org/packages/DurableTask.ActivityProxy)

## Basic usage

### Write activity

```csharp
// Contract for activity
public interface IHelloActivity
{
    Task<string> SayHello(string name);
}

// Implementation of activity
public class HelloActivity : IHelloActivity
{
    [FunctionName(nameof(SayHello))]
    public Task<string> SayHello([ActivityTrigger] string name)
    {
        return Task.FromResult($"Hello {name}!");
    }
}
```

### Create proxy and called methods

```csharp
public class Function1
{
    [FunctionName("Function1")]
    public async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] DurableOrchestrationContext context)
    {
        var outputs = new List<string>();

        var proxy = context.CreateActivityProxy<IHelloActivity>();

        // Replace "hello" with the name of your Durable Activity Function.
        outputs.Add(await proxy.SayHello("Tokyo"));
        outputs.Add(await proxy.SayHello("Seattle"));
        outputs.Add(await proxy.SayHello("London"));

        // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
        return outputs;
    }
}
```
