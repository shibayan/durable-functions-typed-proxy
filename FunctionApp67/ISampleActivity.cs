using System.Threading.Tasks;

namespace FunctionApp67
{
    public interface ISampleActivity
    {
        Task<string> SayHello(string name);

        [RetryOptions("00:00:05", 10)]
        Task<string> HttpGet(string path);
    }
}