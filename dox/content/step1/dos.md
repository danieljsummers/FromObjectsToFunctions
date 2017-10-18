### Dos - Step 1

For this project, we'll also start with `project.json`, bringing in the dependencies we'll need.

    [lang=text]
    "dependencies": {
      "Microsoft.AspNetCore.Owin": "1.0.0",
      "Microsoft.AspNetCore.Server.Kestrel": "1.0.0",
      "Nancy": "2.0.0-barneyrubble"
    },

Nancy strives to provide a Super-Duper-Happy-Path (SDHP), where all you have to do is follow their conventions, and everything will "just work."  (You can also configure every aspect of it; it's only opinionated in its defaults.)  One of these conventions is that the controllers inherit from `NancyModule`, and when they do, no further configuration is required.  So, we create the `Modules` directory, and add `HomeModule.cs`, which looks like this:

    [lang=csharp]
    namespace Dos.Modules
    {
        using Nancy;
    
        public class HomeModule : NancyModule
        {
            public HomeModule() : base()
            {
                Get("/", _ => "Hello World from Nancy C#");
            }
        }
    }

Since we'll be hosting this with Kestrel (via OWIN), we still need a `Startup.cs`, though its `Configure()` method looks a bit different:

    [lang=csharp]
    public void Configure(IApplicationBuilder app) =>
        app.UseOwin(x => x.UseNancy());

(We need to add a using statement for `Nancy.Owin` so that the `UseNancy()` method is visible.)

The `App.cs` file is identical to the one from Uno.

[Back to Step 1](../step1)