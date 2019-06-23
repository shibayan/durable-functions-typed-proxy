# Type-safe activity helper for Durable Functions

[![Build status](https://ci.appveyor.com/api/projects/status/ftq9q7l8wr7ynpn2/branch/master?svg=true)](https://ci.appveyor.com/project/shibayan/durable-functions-activity-proxy/branch/master)

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
    // The return type must be Task or Task <T>
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

        // Create activity proxy with interface
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

### Retry options

```csharp
public interface IHttpGetActivity
{
    // Declarative RetryOptions definition
    [RetryOptions("00:00:05", 10)]
    Task<string> HttpGet(string path);
}

public class HttpGetActivity : IHttpGetActivity
{
    public HttpGetActivity(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private readonly HttpClient _httpClient;

    [FunctionName(nameof(HttpGet))]
    public Task<string> HttpGet([ActivityTrigger] string path)
    {
        return _httpClient.GetStringAsync(path);
    }
}
```

## Advanced usage

### Custom retry handler

```csharp
public static class RetryStrategy
{
    // Implemented custom retry handler
    public static bool HttpError(Exception ex)
    {
        return ex.InnerException is HttpRequestException;
    }
}

public interface IHttpGetActivity
{
    // Set HandlerType and HandlerMethodName
    [RetryOptions("00:00:05", 10, HandlerType = typeof(RetryStrategy), HandlerMethodName = nameof(RetryStrategy.HttpError))]
    Task<string> HttpGet(string path);
}

public class HttpGetActivity : IHttpGetActivity
{
    public HttpGetActivity(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private readonly HttpClient _httpClient;

    [FunctionName(nameof(HttpGet))]
    public Task<string> HttpGet([ActivityTrigger] string path)
    {
        return _httpClient.GetStringAsync(path);
    }
}
```

## Blog

- https://blog.shibayan.jp/entry/20190621/1561114911 (Japanese)

## License

This project is licensed under the [Apache License 2.0](https://github.com/shibayan/durable-functions-activity-proxy/blob/master/LICENSE)
