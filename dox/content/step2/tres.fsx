(*** hide ***)
#r "../../../packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"

namespace Tres.Entities

type Revision = {
  AsOf : int64
  Text : string
  }
with
  static member Empty =
    { AsOf       = 0L
      Text       = ""
      }

(**
### Tres - Step 2

As we make the leap to F#, we're changing things around significantly.  Remember our
[discussion about the flat structure of an F# project](../step1/tres.html)?  Instead of an `Entities` directory with a
lot of little files, we'll define a single `Entities.fs` file in the root of the project.  Don't forget to add it to
the list of compiled files in `project.json`; it should go above `HomeModule.fs`.

Next up, we will change the static classes that we created to eliminate magic strings into modules.  The
`AuthorizationLevel` type in C# looked like:

    [lang=csharp]
    public static class AuthorizationLevel
    {
        const string Administrator = "Administrator";

        const string User = "User";
    }

The F# version (within the namespace `Tres.Entities`):
*)
[<RequireQualifiedAccess>]
module AuthorizationLevel =
  [<Literal>]
  let Administrator = "Administrator"
  [<Literal>]
  let User = "User"
(**
The `RequireQualifiedAccess` attribute means that this module cannot be `open`ed, which means that `Administrator`
cannot ever be construed to be that value; it must be referenced as `AuthorizationLevel.Administrator`.  The
`Literal` attribute means that these values can be used in places where a literal string is required.  (There is a
specific place this will help us when we start writing code around these types.)  Also of note here is the different
way F# defines attributes from the way C# does; instead of `[` `]` pairs, we use `[<` `>]` pairs.

We are also going to change from class types to record types.  Record types can be thought of as `struct`s, though the
comparison is not exact; record types are reference types, not value types, but they cannot be set to null **in code**
_(huge caveat which we'll see in the next step)_ unless explicitly identified.  We're also going to embrace F#'s
immutability-by-default qualities that will save us a heap of null checks (as well as those pesky situations where we
forget to implement them).

As a representative example, consider the `Page` type.  In C#, it looks like this:

    [lang=csharp]
    namespace Uno.Entities
    {
        using Newtonsoft.Json;
        using System.Collections.Generic;
        
        public class Page
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            
            public string WebLogId { get; set; }
            
            public string AuthorId { get; set; }
            
            public string Title { get; set; }
            
            public string Permalink { get; set; }
            
            public long PublishedOn { get; set; }
            
            public long UpdatedOn { get; set; }
            
            public bool ShowInPageList { get; set; }
            
            public string Text { get; set; }
            
            public ICollection<Revision> Revisions { get; set; } = new List<Revision>(); 
        }
    }

It contains strings, for the most part, and a `Revisions` collection.  Now, here's how we'll implement this same thing
in F#:
*)
namespace Tres.Entities

open Newtonsoft.Json
//...
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
(**
The field declarations immediately under the `type` declaration mirror those in our C# version; since they are fields,
though, we don't have to define getters and setters.

F# requires record types to always have all fields defined.  F# also provides a `with` statement (separate from the one
in the code above) that allows us to create a new instance of a record type that has all the fields of our original
ones, only replacing the ones we specify.  So, in C#, while we can do something like

    [lang=csharp]
    var pg = new Page { Title = "Untitled" };

, leaving all the other fields in their otherwise-initialized state, F# will not allow us to do that.  This is where
the `Empty` static property comes in; we can use this to create new pages, while ensuring that we have sensible
defaults for all the other fields.  The equivalent to the above C# statement in F# would be
*)
let pg = { Page.Empty with Title = "Untitled" }
(**
.  Note the default values for `Permalink`: in C#, it's null, but in F#, it's an empty string.  Now, certainly, you can
use `String.IsNullOrEmpty()` to check for both of those, but we'll see some advantages to this lack of nullability as
we continue to develop this project.

A few syntax notes:
- `[]` represents an empty list in F#.  An F# list (as distinguished from `System.Collections.List` or
`System.Collections.Generic.List<T>`) is also an immutable data structure; it consists of a head element, and a tail
list.  It can be constructed by creating a new list with an element as its head and the existing list as its tail, and
deconstructed by processing the head, then processing the head of the tail, etc.  (There are operators and functions to
support that; we'll definitely use those as we go along.)  Items in a list are separated by semicolons;
`[ "one"; "two"; "three" ]` represents a `string list` with three items.  It supports most all the collection
operations you would expect, but there are some differences.
- While not demonstrated here, arrays are defined between `[|` `|]` pairs, also with elements separated by semicolons.

Before continuing on to [Quatro](quatro.html), you should familiarize yourself with the
[types in this step](https://github.com/danieljsummers/FromObjectsToFunctions/tree/step-2/src/3-Nancy-FSharp/Entities.fs).

[Back to Step 2](../step2)
*)