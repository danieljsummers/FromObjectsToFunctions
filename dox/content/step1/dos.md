### Dos - Step 1

For this project, we'll make sure our project file is `Dos.csproj`, and modify it the way we did [for Uno](./uno.html); we'll include one extra dependency to bring in Nancy.

    [lang=xml]
    <PropertyGroup>
      <AssemblyName>Dos</AssemblyName>
      <VersionPrefix>2.0.0</VersionPrefix>
      <OutputType>Exe</OutputType>
      <TargetFramework>netcoreapp2.2</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
      <PackageReference Include="Microsoft.AspNetCore.Owin" Version="2.*" />
      <PackageReference Include="Microsoft.AspNetCore.Server.Kestrel" Version="2.*" />
      <PackageReference Include="Nancy" Version="2.*" />
    </ItemGroup>

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