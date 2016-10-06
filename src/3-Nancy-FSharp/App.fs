namespace Tres

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Nancy
open Nancy.Owin

type Startup() =
  member this.Configure (app : IApplicationBuilder) =
    app.UseOwin (fun x -> x.UseNancy (fun x -> ()) |> ignore) |> ignore

module App = 
  [<EntryPoint>]
  let main argv = 
    use host = (new WebHostBuilder()).UseKestrel().UseStartup<Startup>().Build()
    host.Run()
    0
