using System.Threading.Tasks;

namespace SampleApp.Activities
{
    public interface IAliasActivity
    {
        Task<string> SayHello(string name);
    }
}