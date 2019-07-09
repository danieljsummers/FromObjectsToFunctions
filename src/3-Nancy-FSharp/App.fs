namespace Tres

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Nancy.Owin

type Startup() =
  member __.Configure (app : IApplicationBuilder) =
    app.UseOwin (fun x -> x.UseNancy () |> ignore) |> ignore

module App = 
  [<EntryPoint>]
  let main argv = 
    use host = (new WebHostBuilder()).UseKestrel().UseStartup<Startup>().Build()
    host.Run()
    0
