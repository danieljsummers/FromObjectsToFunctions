(*** hide ***)
#r @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\netstandard\v4.0_2.0.0.0__cc7b13ffcd2ddd51\netstandard.dll"
#r "../../../packages/Nancy/lib/netstandard2.0/Nancy.dll"
#r "../../../packages/Microsoft.AspNetCore.Hosting/lib/netstandard2.0/Microsoft.AspNetCore.Hosting.dll"
#r "../../../packages/Microsoft.AspNetCore.Hosting.Abstractions/lib/netstandard2.0/Microsoft.AspNetCore.Hosting.Abstractions.dll"
#r "../../../packages/Microsoft.AspNetCore.Http.Abstractions/lib/netstandard2.0/Microsoft.AspNetCore.Http.Abstractions.dll"
#r "../../../packages/Microsoft.AspNetCore.Owin/lib/netstandard2.0/Microsoft.AspNetCore.Owin.dll"
#r "../../../packages/Microsoft.AspNetCore.Server.Kestrel/lib/netstandard2.0/Microsoft.AspNetCore.Server.Kestrel.dll"
#r "../../../packages/Microsoft.AspNetCore.Server.Kestrel.Core/lib/netstandard2.0/Microsoft.AspNetCore.Server.Kestrel.Core.dll"

(**
### Tres - Step 1

Here, we're making the leap to F#.  Once we ensure that our project file is named `Tres.fsproj`, the contents of the
file should be the same as they were for [Dos](./dos.html).  F# projects are historically not split into directories, as
compilation order is significant, and having them in the same directory allows the tooling to ensure that the
compilation order is preserved.  With the structure of the `.fsproj` file, this is not necessarily a limitation (though
the tooling still doesn't support it, as of this writing), but we'll follow it for our purposes here.

The module is created as `HomeModule.fs` in the project root:
*)
namespace Tres

open Nancy

type HomeModule() as this =
  inherit NancyModule()

  do
    this.Get("/", fun _ -> "Hello World from Nancy F#")
(**
If you look at [Dos](./dos.html), you can see how the translation occurred:

- "using" becomes "open"
- F# does not express constructors in the way C# folks are used to seeing them.  Parameters to the class are specified
in the type declaration (or a `new` function, which we don't need for our purposes), and then are visible throughout
the class.
- Since we don't have an explicit constructor where we can wire up the `Get()` method call, we accomplish it using a
`do` binding; this is code that will be run every time the class is instantiated.  The `as this` at the end of
`type HomeModule()` allows us to use `this` to refer to the current instance; otherwise, `do` cannot see it.
- This also illustrates the syntax differences in defining lambdas between C# and F#.  F# uses the `fun` keyword to
indicate an anonymous function.  The `_` is used to indicate that we do not care what the parameter is; since this
request doesn't require anything from the `DynamicDictionary` Nancy provides, we don't.

We rename `Program.fs` to `App.fs`, and in this file, we provide the contents from both `Startup.cs` and `App.cs`.
*)
namespace Tres

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Nancy.Owin

type Startup() =
  member __.Configure (app : IApplicationBuilder) =
    app.UseOwin (fun x -> x.UseNancy (fun x -> ()) |> ignore) |> ignore

module App = 
  [<EntryPoint>]
  let main argv = 
    use host = (new WebHostBuilder()).UseKestrel().UseStartup<Startup>().Build()
    host.Run()
    0
(**
The `Startup` class is exactly the same as the C# version, though it appears much differently.  The `UseNancy()` method
returns quite a complex result, but the parameter to the `UseOwin()` method expects an `Action<>`; by definition, this
returns `void`\*.  In F#, there is no implicit throwaway of results\**; you must explicitly mark results that should be
ignored.  `UseNancy` also expects an `Action<>`, so we end up with an extra lambda and two `ignore`s to accomplish the
same thing.

The `App` module is also new.  F# modules can be thought of as static classes (if you use one from C#, that's what they
look like).  An F# source file must start with either a namespace or module declaration; also, any code (`let`, `do`,
`member`, etc.) cannot be simply in a namespace.  We start with the `Tres` namespace so that our `Startup` class's full
name will be `Tres.Startup`, so we have to define a module for our `let` binding / entry point.

At this point, `dotnet build` will fail.  I mentioned compilation order earlier; we've added one file and renamed the
other, but we have yet to tell the compiler about them, or how they should be ordered.  Back in `Tres.fsproj`, between
the `PropertyGroup` and the `ItemGroup`, add the following `ItemGroup`:

    [lang=xml]
    <ItemGroup>
      <Compile Include="HomeModule.fs" />
      <Compile Include="App.fs" />
    </ItemGroup>

(In the future, we'll add updating this list to our discipline of creating a new file.)

Now, we can execute `dotnet run`, watch it start, visit localhost:5000, and see our F# message.

[Back to Step 1](../step1)

---

\* The `unit` type in F# is the parallel to this, but there's more to it than just "something else to call `void`."

\** For example, `StringBuilder.Append()` returns the builder so you can chain calls, but it also mutates the builder,
and you don't have to provide a variable assignment for every call.  In F#, you would either need to provide that, or
pipe the output (`|>`) to `ignore`.
*)