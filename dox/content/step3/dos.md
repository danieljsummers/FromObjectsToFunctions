### Dos - Step 3

This step will follow the same general path as [Uno](./uno.html), but we'll also iterate on what we made there to bring
some of the code into a more functional style.

#### `Dos.csproj` Changes

We only need to add the RethinkDB package to what we had at the end of step 2 (in the dependency `ItemGroup`):

    [lang=text]
    <PackageReference Include="RethinkDb.Driver" Version="2.*" />

Run `dotnet restore` to pull in that dependency.  Also, we'll bring across the entire `Data` directory we created in
**Uno** during this step.  We'll be able to use `Table.cs` and `EnvironmentExtensions.cs` as is (except for changing
the namespace to `Dos.Data`).

#### Configurable Connection

Since `appsettings.json` is a .NET Core thing, we will not use it here.  We can still use JSON to configure our
connection, though; here's the `data-config.json` file:

    [lang=text]
    {
      "Hostname": "my-rethinkdb-server",
      "Database": "O2F2"
    }

To support this, we'll need to change a couple of other things.  First, in our `DataConfig` file, we'll add the
following using statement and static method:

    [lang=csharp]
    using Newtonsoft.Json;
    ...
    public static DataConfig FromJson(string json) => JsonConvert.DeserializeObject<DataConfig>(json);

Now we can create our configuration from this JSON once we read it in.  We're also going to modify the
`CreateConnection()` method; we'll also add some more `using`s and a support property.

    [lang=csharp]
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static RethinkDb.Driver.Net.Connection;
    ...
    private IEnumerable<Func<Builder, Builder>> Blocks
    {
        get
        {
            yield return builder => null == Hostname ? builder : builder.Hostname(Hostname);
            yield return builder => 0 == Port ? builder : builder.Port(Port);
            yield return builder => null == AuthKey ? builder : builder.AuthKey(AuthKey);
            yield return builder => null == Database ? builder : builder.Db(Database);
            yield return builder => 0 == Timeout ? builder : builder.Timeout(Timeout);
        }
    }
    
    public IConnection CreateConnection() =>
        Blocks.Aggregate(RethinkDB.R.Connection(), (builder, block) => block(builder)).Connect();

If this is the first time you've seen code like this, you may be thinking "Why would we do something like that?"  We're
moving from an imperative style, like we used in **Uno**, to a more declarative style.  `Blocks` is an enumeration (or
sequence) where each item yielded takes a connection builder, and returns either the original connection builder it
received or one that has been modified with a configuration parameter.  If you didn't understand that last sentence,
look at the code, then read it again; understanding this structure will pay dividends once we're knee-deep in F#.

Once you understand the `Blocks` property, take a look at the `CreateConnection()` method.  This uses the LINQ method
`Aggregate`, which takes two parameters: an initial state; and a function that will be given the current state and each
item, and will return the "current" state after that item has been processed.  If that makes no sense, imagine you had
a sequence `exSeq` with the letters "A", "B", and "C".  If you were to run
`var str = exSeq.Aggregate("", (state, letter) => String.Format("{0},{1}", state, letter));`, `str` would hold the
string ",A,B,C".  The "initial state" is simply the starting value; but, every iteration must return a value of that
same type.

Hopefully `Aggregate` is making sense at this point.  Taking a forward-looking side trip - we're going to see it, with
a different parameter order, as the `fold` function in our F# code.  You've likely heard the term "map/reduce" - this
describes a process where, given a data collection, you can transform it into a shape you need (map) and distill that
data into the answer you need (reduce).  (Yes, purists, this is a bit of a simplification.)  F# provides `map` and
`reduce` implementations for several collection types; however, `reduce` cannot produce a type different from that of
the underlying collection - `fold` is what does that.

Back from our side trip, what this code does is:

- Seeds `Aggregate` with `RethinkDB.R.Connection()`, which is an instance of `Connection.Builder` _(`Builder` is a
nested type within `RethinkDb.Driver.Net.Connection`; the `using static` makes it visible the way we've used it here.)_
- Loops through each item of our enumeration.  Since each item is a `Func<Builder, Builder>`, we pass the item the
current builder; it returns a builder that may have been further configured ("aggregating" our configuration).
- Once the `Aggregate` has completed, we're ready to call `Connect()` on our connection builder, and return that from
our method.

Seeing a more functional style with C# should help when we start seeing F# collections.

#### Dependency Injection

With Nancy, if you want to add forks to the SDHP, you have to provide a bootstrapper that will handle the startup code.
For most purposes, the best way is to simply override `DefaultNancyBootstrapper`; that way, any code you don't provide
will use the default, and you can call `base` methods from your overridden ones, so all the SDHP magic continues to
work.

Here's the custom bootstrapper we'll use:

    [lang=csharp]
    namespace Dos
    {
        using Dos.Data;
        using Nancy;
        using Nancy.TinyIoc;
        using System.IO;
        
        public class DosBootstrapper : DefaultNancyBootstrapper
        {
            public DosBootstrapper() : base() { }
            
            protected override void ConfigureApplicationContainer(TinyIoCContainer container)
            {
                base.ConfigureApplicationContainer(container);
                
                var cfg = DataConfig.FromJson(File.ReadAllText("data-config.json"));
                var conn = cfg.CreateConnection();
                conn.EstablishEnvironment(cfg.Database).GetAwaiter().GetResult();
                container.Register(conn);
            }
        }
    }

This looks very similar to the code from the ASP.NET Core implementation, with the exception of how we're getting the
configuration.  We're all done except for two minor fixes.  First, we need to tell Nancy to use this bootstrapper
instead of the default.  This is `Startup.cs`:

    [lang=csharp]
    public void Configure(IApplicationBuilder app) =>
        app.UseOwin(x => x.UseNancy(options => options.Bootstrapper = new DosBootstrapper()));

Finally, we need to specify that our `data-config.json` file should be copied to the output directory; otherwise, it
will just sit on the hard drive while you scratch your head trying to figure out why your application can't connect.
_(Ask me how I know...)_  This change is in `project.json`, just under the `emitEntryPoint` declaration (included here
for context):

    [lang=text]
    "emitEntryPoint": true,
    "copyToOutput": {
      "include": "data-config.json"
    }

At this point, you should be able to `dotnet run` and, once the console says that it's listening, you should be able to
see the database, tables, and indexes in the `O2F2` database.

[Back to Step 3](../step3)