using System.Diagnostics;
using System.Threading.Tasks;

namespace CopyTranslation.Native
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new AppService().RunAsync();
        }
    }
}
