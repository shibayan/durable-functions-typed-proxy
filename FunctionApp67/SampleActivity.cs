using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;

namespace FunctionApp67
{
    public class SampleActivity : ISampleActivity
    {
        public SampleActivity(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;

        [FunctionName(nameof(SayHello))]
        public Task<string> SayHello([ActivityTrigger] string name)
        {
            return Task.FromResult($"Hello {name}!");
        }

        [FunctionName(nameof(HttpGet))]
        public async Task<string> HttpGet([ActivityTrigger] string path)
        {
            var response = await _httpClient.GetAsync(path);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}