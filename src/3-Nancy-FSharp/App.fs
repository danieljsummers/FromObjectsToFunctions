namespace Tres

open Indexes
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Nancy
open Nancy.Owin
open Newtonsoft.Json
open Raven.Client.Documents
open Raven.Client.Documents.Indexes
open System.IO

type TresBootstrapper () =
  inherit DefaultNancyBootstrapper ()

  override __.ConfigureApplicationContainer container =
    base.ConfigureApplicationContainer container
    let cfg = File.ReadAllText "data-config.json" |> JsonConvert.DeserializeObject<DataConfig>
    let store = new DocumentStore (Urls = cfg.Urls, Database = cfg.Database)
    container.Register<IDocumentStore> (store.Initialize ()) |> ignore
    IndexCreation.CreateIndexes (typeof<Categories_ByWebLogIdAndSlug>.Assembly, store)

type Startup() =
  member __.Configure (app : IApplicationBuilder) =
    app.UseOwin (fun x -> x.UseNancy (fun opt -> opt.Bootstrapper <- new TresBootstrapper()) |> ignore) |> ignore

module App = 
  [<EntryPoint>]
  let main argv = 
    use host = (new WebHostBuilder()).UseKestrel().UseStartup<Startup>().Build()
    host.Run()
    0
