namespace Quatro

open Chiron
open RethinkDb.Driver
open RethinkDb.Driver.Net
open System

type ConfigParameter =
  | Hostname of string
  | Port     of int
  | AuthKey  of string
  | Timeout  of int
  | Database of string

type DataConfig = { Parameters : ConfigParameter list }
with
  member this.CreateConnection () : IConnection =
    let folder (builder : Connection.Builder) block =
      match block with
      | Hostname x -> builder.Hostname x
      | Port     x -> builder.Port     x
      | AuthKey  x -> builder.AuthKey  x
      | Timeout  x -> builder.Timeout  x
      | Database x -> builder.Db       x
    let bldr =
      this.Parameters
      |> Seq.fold folder (RethinkDB.R.Connection ())
    upcast bldr.Connect()
  member this.Database =
    match this.Parameters
          |> List.filter (fun x -> match x with Database _ -> true | _ -> false)
          |> List.tryHead with
    | Some (Database x) -> x
    | _ -> RethinkDBConstants.DefaultDbName
  static member FromJson json =
    match Json.parse json with
    | Object config ->
        let options =
          config
          |> Map.toList
          |> List.map (fun item ->
              match item with
              | "Hostname", String x -> Hostname x
              | "Port",     Number x -> Port <| int x
              | "AuthKey",  String x -> AuthKey x
              | "Timeout",  Number x -> Timeout <| int x
              | "Database", String x -> Database x
              | key, value ->
                  raise <| InvalidOperationException
                             (sprintf "Unrecognized RethinkDB configuration parameter %s (value %A)" key value))
        { Parameters = options }
    | _ -> { Parameters = [] }

[<RequireQualifiedAccess>]
module Table =
  let Category = "Category"
  let Comment = "Comment"
  let Page = "Page"
  let Post = "Post"
  let User = "User"
  let WebLog = "WebLog"

open RethinkDb.Driver.Ast

[<RequireQualifiedAccess>]
module Data =
  let establishEnvironment database conn =
    let r = RethinkDB.R
    let checkDatabase db =
      async {
        match db with
        | null
        | "" -> ()
        | _ -> let! dbs = r.DbList().RunResultAsync<string list> conn
               match dbs |> List.contains db with
               | true -> ()
               | _ -> do! r.DbCreate(db).RunResultAsync conn
      }
    let checkTables () =
      async {
        let! existing = r.TableList().RunResultAsync<string list> conn
        [ Table.Category; Table.Comment; Table.Page; Table.Post; Table.User; Table.WebLog ]
        |> List.filter (fun tbl -> not (existing |> List.contains tbl))
        |> List.map (fun tbl -> async { do! r.TableCreate(tbl).RunResultAsync conn })
        |> List.iter Async.RunSynchronously
      }
    let checkIndexes () =
      let indexesFor tbl = async { return! r.Table(tbl).IndexList().RunResultAsync<string list> conn }
      let checkCategoryIndexes () =
        async {
          let! indexes = indexesFor Table.Category
          match indexes |> List.contains "WebLogId" with
          | true -> ()
          | _ -> do! r.Table(Table.Category).IndexCreate("WebLogId").RunResultAsync conn
          match indexes |> List.contains "WebLogAndSlug" with
          | true -> ()
          | _ -> do! r.Table(Table.Category)
                      .IndexCreate("WebLogAndSlug", ReqlFunction1 (fun row -> upcast r.Array (row.["WebLogId"], row.["Slug"])))
                      .RunResultAsync conn
          }
      let checkCommentIndexes () =
        async {
          let! indexes = indexesFor Table.Comment
          match indexes |> List.contains "PostId" with
          | true -> ()
          | _ -> do! r.Table(Table.Comment).IndexCreate("PostId").RunResultAsync conn 
          }
      let checkPageIndexes () =
        async {
          let! indexes = indexesFor Table.Page
          match indexes |> List.contains "WebLogId" with
          | true -> ()
          | _ -> do! r.Table(Table.Page).IndexCreate("WebLogId").RunResultAsync conn
          match indexes |> List.contains "WebLogAndPermalink" with
          | true -> ()
          | _ -> do! r.Table(Table.Page)
                      .IndexCreate("WebLogAndPermalink",
                        ReqlFunction1(fun row -> upcast r.Array(row.["WebLogId"], row.["Permalink"])))
                      .RunResultAsync conn
          }
      let checkPostIndexes () =
        async {
          let! indexes = indexesFor Table.Post
          match indexes |> List.contains "WebLogId" with
          | true -> ()
          | _ -> do! r.Table(Table.Post).IndexCreate("WebLogId").RunResultAsync conn
          match indexes |> List.contains "Tags" with
          | true -> ()
          | _ -> do! r.Table(Table.Post).IndexCreate("Tags").OptArg("multi", true).RunResultAsync conn
          }
      let checkUserIndexes () =
        async {
          let! indexes = indexesFor Table.User
          match indexes |> List.contains "EmailAddress" with
          | true -> ()
          | _ -> do! r.Table(Table.User).IndexCreate("EmailAddress").RunResultAsync conn
          }
      async {
        do! checkCategoryIndexes ()
        do! checkCommentIndexes ()
        do! checkPageIndexes ()
        do! checkPostIndexes ()
        do! checkUserIndexes ()
      }
    async {
      do! checkDatabase database
      do! checkTables ()
      do! checkIndexes ()
    }
