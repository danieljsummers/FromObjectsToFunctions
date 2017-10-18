### Uno - Step 3

Our implementation here will fall into two broad categories - defining the configurable connection and table/index
checking code that we can run at startup, and configuring ASP.NET Core's DI container to wire it all up.  Before we get
to that, though, we need to add a few packages to `project.json` (under `dependencies`) for this step.

    [lang=text]
    "Microsoft.Extensions.Configuration.FileExtensions": "1.0.0",
    "Microsoft.Extensions.Configuration.Json": "1.0.0",
    "Microsoft.Extensions.Options.ConfigurationExtensions": "1.0.0",
    "RethinkDb.Driver": "2.3.15"

#### Configurable Connection

Our application will need an instance of RethinkDB's `IConnection` to utilize.  To support our configuration options,
we will make a POCO called `DataConfig`, under a new `Data` directory in our project, and also give it an instance
method to create the connection with the current values.

    [lang=csharp]
    namespace Uno.Data
    {
        using RethinkDb.Driver;
        using RethinkDb.Driver.Net;
        
        public class DataConfig
        {
            public string Hostname { get; set; }
            
            public int Port { get; set; }
            
            public string AuthKey { get; set; }
            
            public int Timeout { get; set; }
            
            public string Database { get; set; }
            
            public IConnection CreateConnection()
            {
                var conn = RethinkDB.R.Connection();
                
                if (null != Hostname) { conn = conn.Hostname(Hostname); }
                if (0 != Port) { conn = conn.Port(Port); }
                if (null != AuthKey) { conn = conn.AuthKey(AuthKey); }
                if (null != Database) { conn = conn.Db(Database); }
                if (0 != Timeout) { conn = conn.Timeout(Timeout); }
                
                return conn.Connect();
            }
        }
    }

Note that the connection builder uses a fluent interface.  We just as well could have chained all of these together,
using defaults where we had no data, like so:

    [lang=csharp]
    RethinkDB.R.Connection()
        .Hostname(Hostname ?? RethinkDBConstants.DefaultHostname)
        .Port(0 == Port ? RethinkDBConstants.DefaultPort : Port)
        ...etc...
        .Connect();

We could then actually define this as a fat-arrow (`=>`) function and omit the return.  If C# were our final
destination, that's a fine implementation; of course, it's not, and I've structured it this way to illustrate that we
really only have to call the configuration methods for properties that we've specified in our JSON file.

Note also that we are mutating the `conn` variable with the result of each builder call.  Do we need to do this?  I
have no idea; if the C# driver is (under the hood) mutating itself, we don't; if it's returning a new version of the
builder with a change made (the F#/immutable way of doing things), we do.  I certainly could find out _(yay, open
source!)_, but it's an implementation detail we don't need to know.  It's not wrong to do it this way, and in future
implementations, we will be accomplishing the same thing without using mutation - at least in our code.

#### Tables

RethinkDB uses the term "table" to represent a collection of documents.  Other document databases use the term
"collection" or "document store"; this is the rough equivalent of a relational table.  Of course, the difference here
is that the documents do not all have to conform to the same schema.  `Data/Table.cs` contains C# constants we will use
to reference our tables.

#### Ensuring Tables and Indexes Exist

Many of the new APIs that are provided within .NET Core are implemented as extension methods on existing objects.
Since `IConnection` represents our connection to RethinkDB, we'll target that type for our extension methods.  We
create the `EnvironmentExtensions.cs` file under the `Data` directory, and define it as a `public static` class.

In our overall plan for step 3, we defined several types of queries we want to be able to run against these tables.
While RethinkDB will create a table the first time you try to store a document in it, we cannot define indexes against
them in this scenario.  Indexes are the way RethinkDB avoids a complete table scan for documents; the concept is very
similar to an index on a relational table.  Since we need to define these indexes before our application can use them,
we'll need make sure the tables exist, so we can create indexes against them.

We will not go line-by-line through `EnvironmentExtensions.cs`; it's rather straightforward, and simply ensures that
the database, tables, and indexes exist.  It is our first exposure to the RethinkDB API, though, so be sure to
[review the source](https://github.com/danieljsummers/FromObjectsToFunctions/tree/step-3/src/1-AspNetCore-CSharp/Data/EnvironmentExtensions.cs)
to ensure you get a sense of how data access is designed to work in the RethinkDB driver.

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