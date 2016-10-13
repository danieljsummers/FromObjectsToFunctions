namespace Uno
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System.Threading.Tasks;
    using Uno.Data;

    public class Startup
    {
        public static IConfigurationRoot Configuration { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            System.Console.WriteLine("Content root = " + env.ContentRootPath);
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<DataConfig>(Configuration.GetSection("RethinkDB"));
            
            var cfg = services.BuildServiceProvider().GetService<IOptions<DataConfig>>().Value;
            var conn = cfg.CreateConnection();
            conn.EstablishEnvironment(cfg.Database).GetAwaiter().GetResult();
            services.AddSingleton(conn);
        }
        
        public void Configure(IApplicationBuilder app) =>
            app.Run(async context => await context.Response.WriteAsync("Hello World from ASP.NET Core"));
    }
}