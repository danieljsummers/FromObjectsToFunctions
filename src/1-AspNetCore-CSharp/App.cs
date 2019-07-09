using Microsoft.AspNetCore.Hosting;

namespace Uno
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