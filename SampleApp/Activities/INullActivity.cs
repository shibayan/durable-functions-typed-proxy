using System.Threading.Tasks;

namespace SampleApp.Activities
{
    public interface INullActivity
    {
        Task Nop(object input = null);
    }
}
