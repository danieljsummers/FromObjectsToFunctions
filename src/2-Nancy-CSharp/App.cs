using Microsoft.AspNetCore.Hosting;

namespace Dos
{
    public class App
    {
        public static void Main(string[] args)
        {
            using (var host = new WebHostBuilder().UseKestrel().UseStartup<Startup>().Build())
            {
                host.Run();
            }
        }
    }
}
