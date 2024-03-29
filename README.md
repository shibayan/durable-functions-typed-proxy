# Type-safe activity helper for Durable Functions

[![Build](https://github.com/shibayan/durable-functions-typed-proxy/workflows/Build/badge.svg)](https://github.com/shibayan/durable-functions-typed-proxy/actions/workflows/build.yml)
[![Downloads](https://badgen.net/nuget/dt/DurableTask.TypedProxy)](https://www.nuget.org/packages/DurableTask.TypedProxy/)
[![NuGet](https://badgen.net/nuget/v/DurableTask.TypedProxy)](https://www.nuget.org/packages/DurableTask.TypedProxy/)
[![License](https://badgen.net/github/license/shibayan/durable-functions-typed-proxy)](https://github.com/shibayan/durable-functions-typed-proxy/blob/master/LICENSE)

## Basic usage

### 1. Implement the activity

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

### 2. Create type-safe proxy and called methods

```csharp
public class Function1
{
    [FunctionName("Function1")]
    public async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] DurableOrchestrationContext context)
    {
        var outputs = new List<string>();

        // Create type-safe activity proxy with interface
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

## Advanced usage

### Use ILogger

```csharp
public class HelloActivity : IHelloActivity
{
    public HelloActivity(ILoggerFactory loggerFactory)
    {
        // Create logger instance
        _logger = loggerFactory.CreateLogger<HelloActivity>();
    }
    
    private readonly ILogger _logger;

    [FunctionName(nameof(SayHello))]
    public Task<string> SayHello([ActivityTrigger] string name)
    {
        _logger.LogInformation($"Saying hello to {name}.");
        return Task.FromResult($"Hello {name}!");
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

    // In case of failure, retry is performed transparently
    [FunctionName(nameof(HttpGet))]
    public Task<string> HttpGet([ActivityTrigger] string path)
    {
        return _httpClient.GetStringAsync(path);
    }
}
```

### Custom retry handler

```csharp
public static class RetryStrategy
{
    // Implement custom retry handler
    public static bool HttpError(Exception ex)
    {
        return ex.InnerException is HttpRequestException;
    }
}

public interface IHttpGetActivity
{
    // Must be setting HandlerType and HandlerMethodName
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

This project is licensed under the [Apache License 2.0](https://github.com/shibayan/durable-functions-typed-proxy/blob/master/LICENSE)
