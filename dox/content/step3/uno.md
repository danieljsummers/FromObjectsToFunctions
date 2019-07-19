### Uno - Step 3

Our implementation here will fall into two broad categories - configuring the connection and adding it to the ASP.NET Core's DI container, then adding the indexing code. Before we get to that, though, we need to add a few packages to `Uno.csproj` (under the dependency `ItemGroup`) for this step.

    [lang=xml]
    <PackageReference Include="Microsoft.Extensions.Configuration.FileExtensions" Version="2.*" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="2.*" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="2.*" />
    <PackageReference Include="RavenDb.Client" Version="4.*" />

#### Create the Database

If you run RavenDB in interactive mode, it should launch a browser with RavenDB Studio; if you have it running as a service on your local machine, go to http://localhost:8080. Using the studio, create a database called "O2F1".

#### Configuring the Connection and Adding to DI

We will store our connection settings with the other configuration for the application. The standard .NET Core name for such a file is `appsettings.json`, so we create one with the following values:

    [lang=json]
    {
      "RavenDB": {
        "Url": "http://localhost:8080",
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

The naming convention for indexes within RavenDB is `[collection]/By[field]`. The index description below defines an index that allows us to query categories by web log Id and slug.

    [lang=csharp]
    using Raven.Client.Documents.Indexes;
    using System.Linq;
    using Uno.Entities;

    namespace Uno.Data.Indexes
    {
        public class Categories_ByWebLogIdAndSlug : AbstractIndexCreationTask<Category>
        {
            public Categories_ByWebLogIdAndSlug()
            {
                Map = categories => from category in categories
                                    select new
                                    {
                                        category.WebLogId,
                                        category.Slug
                                    };
            }
        }
    }

Now, let's revisit `Startup.cs`. The RavenDB client has a nice feature where it will scan assemblies for these indexes, and automatically create them. We'll use the name of this index to accomplish the registration.

    [lang=csharp]
    [add]
    using Raven.Client.Documents.Indexes;
    using Uno.Data.Indexes;
    [/add]

    [in ConfigureServices(), after the call to .AddSingleton()]
        IndexCreation.CreateIndexes(typeof(Categories_ByWebLogIdAndSlug).Assembly, store);
    [/end]

Now, if we build and run our application, then use RavenDB studio to look at the indexes for the `O2F1` database, we should be able to see the indexes we specified.

[Back to Step 3](../step3)