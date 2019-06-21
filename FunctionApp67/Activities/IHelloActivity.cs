using System.Threading.Tasks;

namespace FunctionApp67.Activities
{
    public interface IHelloActivity
    {
        Task<string> SayHello(string name);
    }
}