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

Having [already made the leap to F#](tres.html), we will now do our Hello World in Freya.  This is adapted from the
"Getting Started" article on their site.

First up, we'll attack `project.json`.  This one will require more modifications\* than the others.  Let's start
with the dependencies section:

    [lang=text]
    "dependencies": {
      "Freya": "3.0.0-rc01",
      "Microsoft.AspNetCore.Owin": "1.0.0",
      "Microsoft.AspNetCore.Server.Kestrel": "1.0.0",
      "Microsoft.NETCore.Portable.Compatibility": "1.0.1"
    },

Freya should be self-explanatory, and we've seen the Owin and Kestrel imports before.  The new one is the last one, and
this package provides fill-ins for some types that used to be defined in `mscorlib` (the big library-to-rule-them-all
that went away between the full .NET framework and .NET Core).

The frameworks sections needs some attention as well:

    [lang=text]
    "frameworks": {
      "netcoreapp1.0": {
        "dependencies": {
          "Microsoft.NETCore.App": {
            "type": "platform",
            "version": "1.0.1"
          },
          "Microsoft.FSharp.Core.netcore": "1.0.0-alpha-160831"
        },
        "imports": [
          "portable-net45+win8+dnxcore50",
          "portable-net45+win8",
          "net452",
          "dnxcore50"
        ]
      }
    },

These imports allow us to use some Freya dependencies that target Portable Class Libraries (PCLs) that are supported by
.NET Core.  Finally, while we're there, change "Program.fs" to "App.fs" in the compiled file list, as we'll rename
`Program.fs` to `App.fs` to remain consistent among the projects.

Now, for `App.fs`:
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

---

\* - Huge props go to @neoeinstein for documenting these settings in
[this Gist](https://gist.github.com/neoeinstein/66c2c8ace158b3e701e206e172e91f8b).  I had not seen the PCL compat
package _or_ an import for "net452" in .NET Core before this.
*)