### Uno - Step 3

Our implementation here will fall into two broad categories - configuring the connection and adding it to the ASP.NET Core's DI container, then adding the indexing code. Before we get to that, though, we need to add a few packages to `Uno.csproj` (under the dependency `ItemGroup`) for this step.

    [lang=xml]
    <PackageReference Include="Microsoft.Extensions.Configuration.FileExtensions" Version="2.*" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="2.*" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="2.*" />
    <PackageReference Include="RavenDb.Client" Version="4.*" />

#### Create the Database

If you run RavenDB in interactive mode, it should launch a browser with RavenDB Studio; if you have it running as a service on your local machine, go to http://localhost:8082. Using the studio, create a database called "O2F1".

#### Configuring the Connection and Adding to DI

We will store our connection settings with the other configuration for the application. The standard .NET Core name for such a file is `appsettings.json`, so we create one with the following values:

    [lang=json]
    {
      "RavenDB": {
        "Url": "http://localhost:8082",
        "Database": "O2F1"
      }
    }

When we were doing our quick-and-dirty "Hello World" in step 1, we had very minimal content in `Startup.cs`.  Now, we'll flesh that out a little more.

    [lang=csharp]
    [add]
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Raven.Client.Documents;
    [/add]
    
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
        }

This does the following:

- In the constructor, creates a configuration tree that is a union of `appsettings.json`, `appsettings.{environment}.json`, and environment variables (each of those overriding the prior one if settings are specified in both)
- In `ConfigureServices`, gets the `RavenDB` configuration sections, uses it to configure the `DocumentStore` instance, and registers the output of its `Initialize` method as the `IDocumentStore` singleton in the DI container.

We'll come back to this file, but we need to write some more code first.

#### Defining Collections

RavenDB creates document collection names using the plural of the name of the type being stored - ex., a `Post` would go in the `Posts` collection. Its Ids also follow the form `[collection]/[id]`, so post 123 would have the document Id `Posts/123`.  `Data/Collection.cs` contains C# constants we will use to reference our collections. It also contains two utility methods: one for creating a document Id from a collection name and a `Guid`, and the other for deriving the collection name and Id from a document Id.

#### Ensuring Indexes Exist

RavenDB provides a means of creating strongly-typed indexes as classes that extend `AbstractIndexCreationTask<T>`; these definitions can be used to both define and query indexes. We will create these in the `Uno.Data.Indexes` namespace. You can [review all the files there](https://github.com/danieljsummers/FromObjectsToFunctions/tree/v2-step-3/src/1-AspNetCore-CSharp/Data/Indexes/), but we'll look at one example here.

The naming convention for indexes within RavenDB is `[collection]/By[field]`. The index description below defines an index that allows us to query categories by web log Id.

    [lang=csharp]
    using Raven.Client.Documents.Indexes;
    using System.Linq;
    using Uno.Entities;

    namespace Uno.Data.Indexes
    {
        public class Categories_ByWebLogId : AbstractIndexCreationTask<Category>
        {
            public Categories_ByWebLogId()
            {
                Map = categories => from category in categories select category.WebLogId;
            }
        }
    }

**TODO** stopped here


#### Dependency Injection

Now that we have defined our connection, and a method to make sure we have the data environment we need, we need a
connection.  `appsettings.json` is the standard .NET Core name for the configuration file, so we create one with the
following values:

    [lang=text]
    {
      "RethinkDB": {
        "Hostname": "my-rethinkdb-server",
        "Database": "O2F1"
      }
    }

The database name, here `O2F1`, will be different in each of our examples; this way, we can verify that each of our
instances created the tables and indexes correctly.

When we were doing our quick-and-dirty "Hello World" in step 1, we had very minimal content in `Startup.cs`.  Now,
we'll flesh that out a little more.

    [lang=csharp]
    [add]
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Uno.Data;
    [/add]
    
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
            services.AddOptions();
            services.Configure<DataConfig>(Configuration.GetSection("RethinkDB"));
            
            var cfg = services.BuildServiceProvider().GetService<IOptions<DataConfig>>().Value;
            var conn = cfg.CreateConnection();
            conn.EstablishEnvironment(cfg.Database).GetAwaiter().GetResult();
            services.AddSingleton(conn);
        }

This does the following:

- Creates a configuration tree that is a union of `appsettings.json`, `appsettings.{environment}.json`, and environment
variables (each of those overriding the prior one if settings are specified in both)
- Establishes the new `Options` API, registers our `DataConfig` as an option set, and specifies that it should be
obtained from the `RethinkDB` section of the configuration
- Creates a connection based on our configuration
- Runs the `EstablishEnvironment` extension method, so that when we're done, we have the tables and indexes we expect
_(since it's an `async` method, we use the `.GetAwaiter().GetResult()` chain so we don't have to define
`ConfigureServices` as `async`)_
- Registers our `IConnection` for injection

Now, if we build and run our application, then use RethinkDB's administration site to look at our server, we should now
see an `O2F1` database created, along with our tables and indexes.

[Back to Step 3](../step3)