namespace Tres

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Nancy
open Nancy.Owin
open RethinkDb.Driver.Net
open System.IO

type TresBootstrapper() =
  inherit DefaultNancyBootstrapper()

  override this.ConfigureApplicationContainer container =
    base.ConfigureApplicationContainer container
    let cfg = DataConfig.FromJson (File.ReadAllText "data-config.json")
    let conn = cfg.CreateConnection ()
    conn.EstablishEnvironment cfg.Database |> Async.RunSynchronously
    container.Register<IConnection> conn |> ignore

type Startup() =
  member this.Configure (app : IApplicationBuilder) =
    app.UseOwin (fun x -> x.UseNancy (fun opt -> opt.Bootstrapper <- new TresBootstrapper()) |> ignore) |> ignore

module App = 
  [<EntryPoint>]
  let main argv = 
    use host = (new WebHostBuilder()).UseKestrel().UseStartup<Startup>().Build()
    host.Run()
    0
