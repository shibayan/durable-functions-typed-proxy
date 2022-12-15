using System.Net.Http;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using SampleApp;

[assembly: FunctionsStartup(typeof(Startup))]

namespace SampleApp;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton(new HttpClient());
    }
}
