namespace Tres.Entities

open Newtonsoft.Json

[<RequireQualifiedAccess>]
module AuthorizationLevel =
  [<Literal>]
  let Administrator = "Administrator"
  [<Literal>]
  let User = "User"

[<RequireQualifiedAccess>]
module PostStatus =
  [<Literal>]
  let Draft = "Draft"
  [<Literal>]
  let Published = "Published"

[<RequireQualifiedAccess>]
module CommentStatus =
  [<Literal>]
  let Approved = "Approved"
  [<Literal>]
  let Pending = "Pending"
  [<Literal>]
  let Spam = "Spam"

type Revision = {
  AsOf : int64
  Text : string
  }
with
  static member Empty =
    { AsOf       = 0L
      Text       = ""
      }

type Page = {
  [<JsonProperty("id")>]
  Id : string
  WebLogId : string
  AuthorId : string
  Title : string
  Permalink : string
  PublishedOn : int64
  UpdatedOn : int64
  ShowInPageList : bool
  Text : string
  Revisions : Revision list
  }
with
  static member Empty = 
    { Id             = ""
      WebLogId       = ""
      AuthorId       = ""
      Title          = ""
      Permalink      = ""
      PublishedOn    = 0L
      UpdatedOn      = 0L
      ShowInPageList = false
      Text           = ""
      Revisions      = []
      }

type WebLog = {
  [<JsonProperty("id")>]
  Id : string
  Name : string
  Subtitle : string option
  DefaultPage : string
  ThemePath : string
  UrlBase : string
  TimeZone : string
  }
with
  /// An empty web log
  static member Empty =
    { Id          = ""
      Name        = ""
      Subtitle    = None
      DefaultPage = ""
      ThemePath   = "default"
      UrlBase     = ""
      TimeZone    = "America/New_York"
      }

type Authorization = {
  WebLogId : string
  Level : string
  }

type User = {
  [<JsonProperty("id")>]
  Id : string
  EmailAddress : string
  PasswordHash : string
  FirstName : string
  LastName : string
  PreferredName : string
  Url : string option
  Authorizations : Authorization list
  }
with
  static member Empty =
    { Id             = ""
      EmailAddress   = ""
      FirstName      = ""
      LastName       = ""
      PreferredName  = ""
      PasswordHash   = ""
      Url            = None
      Authorizations = []
      }

type Category = {
  [<JsonProperty("id")>]
  Id : string
  WebLogId : string
  Name : string
  Slug : string
  Description : string option
  ParentId : string option
  Children : string list
  }
with
  static member Empty =
    { Id          = "new"
      WebLogId    = ""
      Name        = ""
      Slug        = ""
      Description = None
      ParentId    = None
      Children    = []
      }

type Comment = {
  [<JsonProperty("id")>]
  Id : string
  PostId : string
  InReplyToId : string option
  Name : string
  Email : string
  Url : string option
  Status : string
  PostedOn : int64
  Text : string
  }
with
  static member Empty =
    { Id          = ""
      PostId      = ""
      InReplyToId = None
      Name        = ""
      Email       = ""
      Url         = None
      Status      = CommentStatus.Pending
      PostedOn    = 0L
      Text        = ""
      }

type Post = {
  [<JsonProperty("id")>]
  Id : string
  WebLogId : string
  AuthorId : string
  Status : string
  Title : string
  Permalink : string
  PublishedOn : int64
  UpdatedOn : int64
  Text : string
  CategoryIds : string list
  Tags : string list
  PriorPermalinks : string list
  Revisions : Revision list
  }
with
  static member Empty =
    { Id              = "new"
      WebLogId        = ""
      AuthorId        = ""
      Status          = PostStatus.Draft
      Title           = ""
      Permalink       = ""
      PublishedOn     = 0L
      UpdatedOn       = 0L
      Text            = ""
      CategoryIds     = []
      Tags            = []
      PriorPermalinks = []
      Revisions       = []
      }
