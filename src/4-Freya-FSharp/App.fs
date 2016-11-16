namespace Quatro

open Freya.Core
open Freya.Machines.Http
open Freya.Routers.Uri.Template
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Quatro.Reader
open System.IO

module App =
  let lazyCfg = lazy (File.ReadAllText "data-config.json" |> DataConfig.FromJson)
  let cfg = lazyCfg.Force()
  let deps = {
    new IDependencies with
      member __.Conn
        with get () =
          let conn = lazy (cfg.CreateConnection ())
          conn.Force()
  }

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

  type Startup () =
    member __.Configure (app : IApplicationBuilder) =
      let freyaOwin = OwinMidFunc.ofFreya router
      app.UseOwin (fun p -> p.Invoke freyaOwin) |> ignore

  [<EntryPoint>]
  let main _ =
    (*let initDb (conn : IConnection) = conn.EstablishEnvironment cfg.Database |> Async.RunSynchronously 
    let start = liftDep getConn initDb
    start |> run deps *)
    liftDep getConn (Data.establishEnvironment cfg.Database >> Async.RunSynchronously)
    |> run deps
    use host = (new WebHostBuilder()).UseKestrel().UseStartup<Startup>().Build()
    host.Run()
    0