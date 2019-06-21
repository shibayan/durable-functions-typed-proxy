using System.Threading.Tasks;

namespace SampleApp.Activities
{
    public interface IHelloActivity
    {
        Task<string> SayHello(string name);
    }
}