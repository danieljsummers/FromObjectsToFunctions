### Uno - Step 1

_NOTE: While there is a "web" target for C#, it pulls in a lot of files that I'd rather not go through and explain.  We
will not be using Entity Framework for anything, and though this application will use some of the Identity features of
ASP.NET Core MVC, we will not be using its membership features.  Since all of that is out of scope for this effort, and
all of this is in the "web" template, we won't use it._  😃

To start, we'll open `project.json` and add the dependencies we'll need:

    [lang=text]
    "dependencies": {
      "Microsoft.AspNetCore.Owin": "1.0.0",
      "Microsoft.AspNetCore.Server.Kestrel": "1.0.0"
    },

`dotnet restore` fixes up the actual packages.  Next, we'll create the `Startup.cs` file.  Within its `Configure` method, we'll do a very basic lambda to return a string:

    [lang=csharp]
    public void Configure(IApplicationBuilder app) =>
        app.Run(async context => await context.Response.WriteAsync("Hello World from ASP.NET Core"));

(We put in using statements for `Microsoft.AspNetCore.Builder` to make the `IApplicationBuilder` visible and `Microsoft.AspNetCore.Http` to expose the `WriteAsync()` method on the `Response` object.)

We'll rename `Program.cs` to `App.cs`.  (Why?  Well - why not?)  Then, within the `Main()` method, we'll construct a Kestrel instance and run it.

    [lang=csharp]
    using (var host = new WebHostBuilder().UseKestrel().UseStartup<Startup>().Build())
    {
        host.Run();
    }

(Most demos don't show the web host wrapped in a using block; it's `IDisposable`, though, so it's a good idea.)

At this point, `dotnet run` should give us a successful startup, and browsing to localhost:5000 returns our greeting.

[Back to Step 1](../step1)