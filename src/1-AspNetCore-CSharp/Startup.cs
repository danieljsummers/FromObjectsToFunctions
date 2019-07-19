using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Uno.Data.Indexes;

namespace Uno
{
    public class Startup
    {
        public static IConfigurationRoot Configuration { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var cfg = Configuration.GetSection("RavenDB");
            var store = new DocumentStore
            {
                Urls = new[] { cfg["Url"] },
                Database = cfg["Database"]
            };
            services.AddSingleton(store.Initialize());
            IndexCreation.CreateIndexes(typeof(Categories_ByWebLogIdAndSlug).Assembly, store);
        }

        public void Configure(IApplicationBuilder app) =>
            app.Run(async context => await context.Response.WriteAsync("Hello World from ASP.NET Core"));
    }
}