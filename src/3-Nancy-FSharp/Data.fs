namespace Tres

open Newtonsoft.Json
open System

type DataConfig =
  { Url : string
    Database : string
    }
  with
    [<JsonIgnore>]
    member this.Urls = [| this.Url |]

[<RequireQualifiedAccess>]
module Collection =
  let Category = "Categories"
  let Comment  = "Comments"
  let Page     = "Pages"
  let Post     = "Posts"
  let User     = "Users"
  let WebLog   = "WebLogs"
  
  let IdFor coll (docId : Guid) = sprintf "%s/%s" coll (string docId)

  let FromId docId =
    try
      let parts = (match isNull docId with true -> "" | false -> docId).Split '/'
      match parts.Length with
      | 2 -> parts.[0], Guid.Parse parts.[1]
      | _ -> "", Guid.Empty
    with :?FormatException -> "", Guid.Empty
