namespace Quatro

open Chiron
open RethinkDb.Driver
open RethinkDb.Driver.Ast
open RethinkDb.Driver.Net

type ConfigParameter =
  | Hostname of string
  | Port     of int
  | AuthKey  of string
  | Timeout  of int
  | Database of string

type DataConfig = { Parameters : ConfigParameter list }
with
  member this.CreateConnection () : IConnection =
    let folder (builder : Connection.Builder) (block : ConfigParameter) =
      match block with
      | Hostname x -> builder.Hostname x
      | Port x -> builder.Port x
      | AuthKey x -> builder.AuthKey x
      | Timeout x -> builder.Timeout x
      | Database x -> builder.Db x
    let bldr =
      this.Parameters
      |> Seq.fold folder (RethinkDB.R.Connection())
    upcast bldr.Connect()
  static member FromJson json =
    Json.parse json


[<RequireQualifiedAccess>]
module Table =
  let Category = "Category"
  let Comment = "Comment"
  let Page = "Page"
  let Post = "Post"
  let User = "User"
  let WebLog = "WebLog"

[<AutoOpen>]
module DataExtensions =
  type IConnection with
    member this.EstablishEnvironment database =
      let r = RethinkDB.R
      let checkDatabase db =
        async {
          match db with
          | null
          | "" -> ()
          | _ -> let! dbs = r.DbList().RunResultAsync<string list> this
                 match dbs |> List.contains db with
                 | true -> ()
                 | _ -> do! r.DbCreate(db).RunResultAsync this
        }
      let checkTables () =
        async {
          let! existing = r.TableList().RunResultAsync<string list> this
          [ Table.Category; Table.Comment; Table.Page; Table.Post; Table.User; Table.WebLog ]
          |> List.filter (fun tbl -> not (existing |> List.contains tbl))
          |> List.map (fun tbl -> async { do! r.TableCreate(tbl).RunResultAsync this })
          |> List.iter Async.RunSynchronously
        }
      let checkIndexes () =
        let indexesFor tbl = async { return! r.Table(tbl).IndexList().RunResultAsync<string list> this }
        let checkCategoryIndexes () =
          async {
            let! indexes = indexesFor Table.Category
            match indexes |> List.contains "WebLogId" with
            | true -> ()
            | _ -> do! r.Table(Table.Category).IndexCreate("WebLogId").RunResultAsync this
            match indexes |> List.contains "WebLogAndSlug" with
            | true -> ()
            | _ -> do! r.Table(Table.Category)
                        .IndexCreate("WebLogAndSlug", ReqlFunction1(fun row -> upcast r.Array(row.["WebLogId"], row.["Slug"])))
                        .RunResultAsync this
            }
        let checkCommentIndexes () =
          async {
            let! indexes = indexesFor Table.Comment
            match indexes |> List.contains "PostId" with
            | true -> ()
            | _ -> do! r.Table(Table.Comment).IndexCreate("PostId").RunResultAsync this 
            }
        let checkPageIndexes () =
          async {
            let! indexes = indexesFor Table.Page
            match indexes |> List.contains "WebLogId" with
            | true -> ()
            | _ -> do! r.Table(Table.Page).IndexCreate("WebLogId").RunResultAsync this
            match indexes |> List.contains "WebLogAndPermalink" with
            | true -> ()
            | _ -> do! r.Table(Table.Page)
                        .IndexCreate("WebLogAndPermalink",
                          ReqlFunction1(fun row -> upcast r.Array(row.["WebLogId"], row.["Permalink"])))
                        .RunResultAsync this
            }
        let checkPostIndexes () =
          async {
            let! indexes = indexesFor Table.Post
            match indexes |> List.contains "WebLogId" with
            | true -> ()
            | _ -> do! r.Table(Table.Post).IndexCreate("WebLogId").RunResultAsync this
            match indexes |> List.contains "Tags" with
            | true -> ()
            | _ -> do! r.Table(Table.Post).IndexCreate("Tags").OptArg("multi", true).RunResultAsync this
            }
        let checkUserIndexes () =
          async {
            let! indexes = indexesFor Table.User
            match indexes |> List.contains "EmailAddress" with
            | true -> ()
            | _ -> do! r.Table(Table.User).IndexCreate("EmailAddress").RunResultAsync this
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
