(*** hide ***)
#r "../../../packages/Freya.Core/lib/net452/Freya.Core.dll"
#r "../../../packages/Freya.Machines.Http/lib/net452/Freya.Machines.Http.dll"
#r "../../../packages/Freya.Routers.Uri.Template/lib/net452/Freya.Routers.Uri.Template.dll"
#r "../../../packages/Freya.Types.Uri.Template/lib/net452/Freya.Types.Uri.Template.dll"
#r "../../../packages/Microsoft.AspNetCore.Hosting/lib/net451/Microsoft.AspNetCore.Hosting.dll"
#r "../../../packages/Microsoft.AspNetCore.Hosting.Abstractions/lib/net451/Microsoft.AspNetCore.Hosting.Abstractions.dll"
#r "../../../packages/Microsoft.AspNetCore.Http.Abstractions/lib/net451/Microsoft.AspNetCore.Http.Abstractions.dll"
#r "../../../packages/Microsoft.AspNetCore.Owin/lib/net451/Microsoft.AspNetCore.Owin.dll"
#r "../../../packages/Microsoft.AspNetCore.Server.Kestrel/lib/net451/Microsoft.AspNetCore.Server.Kestrel.dll"

(**
### Quatro - Step 1

Having [already made the leap to F#](./tres.html), we will now do our Hello World in Freya.  Thanks to the hard work of
Microsoft on .NET Core 2, this process exactly mirrors what we did with Tres, just with a Freya dependency instead of
one for Nancy:

    [lang=text]
    <ItemGroup>
      <PackageReference Include="Freya" Version="4.0.0-alpha-*" IncludePrerelease="true" />
      <PackageReference Include="Microsoft.AspNetCore.Owin" Version="2.*" />
      <PackageReference Include="Microsoft.AspNetCore.Server.Kestrel" Version="2.*" />
    </ItemGroup>

We'll go ahead and rename `Program.fs` to `App.fs` to remain consistent among the projects, and tell the compiler about
it:

    [lang=text]
    <ItemGroup>
      <Compile Include="App.fs" />
    </ItemGroup>

Now, let's actually write `App.fs`:
*)
namespace Quatro

open Freya.Core
open Freya.Machines.Http
open Freya.Routers.Uri.Template
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
(**
`Freya.Core` gives us the `freya` computation expression, which we will use for the main part of our request handling.
`Freya.Machines.Http` provides the `freyaMachine` computation expression, which allows us to define our
request-response.  `Freya.Routers.Uri.Template` provides the `freyaRouter` computation expression, where we assign an
HTTP machine to a URL route pattern.

Continuing on...
*)
module App =
  let hello =
    freya {
      return Represent.text "Hello World from Freya"
      }

  let machine =
    freyaMachine {
      handleOk hello
      }

  let router =
    freyaRouter {
      resource "/" machine
      }
(**
This code uses the three expressions described above to define the response (hard-coded for now), the machine that uses
it for its OK response, and the route that uses the machine.

Still within `module App =`...
*)
  type Startup () =
    member __.Configure (app : IApplicationBuilder) =
      let freyaOwin = OwinMidFunc.ofFreya (UriTemplateRouter.Freya router)
      app.UseOwin (fun p -> p.Invoke freyaOwin) |> ignore

  [<EntryPoint>]
  let main _ =
    use host = (new WebHostBuilder()).UseKestrel().UseStartup<Startup>().Build()
    host.Run()
    0
(**
This is the familiar `Startup` class from Tres, except that the `Configure()` method uses the Freya implementation
instead of the Nancy implementation.  Notice that the middleware function uses the router as the hook into the
pipeline; that is how we get the OWIN request to be handled by Freya.  Notice how much closer to idiomatic F# this code
has become; the only place we had to `ignore` anything was the "seam" where we interoperated with the OWIN library.

`dotnet run` should succeed at this point, and localhost:5000 should display our Hello World message.

[Back to Step 1](../step1)
*)