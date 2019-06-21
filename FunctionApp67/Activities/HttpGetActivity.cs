using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;

namespace FunctionApp67.Activities
{
    public class HttpGetActivity : IHttpGetActivity
    {
        public HttpGetActivity(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;

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
