(*** hide ***)
#r "../../../packages/Microsoft.AspNetCore.Http.Abstractions/lib/net451/Microsoft.AspNetCore.Http.Abstractions.dll"
#r "../../../packages/Microsoft.AspNetCore.Owin/lib/net451/Microsoft.AspNetCore.Owin.dll"
#r "../../../packages/Nancy/lib/net452/Nancy.dll"
#r "../../../packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
#r "../../../packages/RethinkDb.Driver/lib/net45/RethinkDb.Driver.dll"

(**
### Tres - Step 3

We'll start out, as we did with **Dos**, with adding the RethinkDB dependency to `project.json`'s `dependencies`
section:

    [lang=text]
    "RethinkDb.Driver": "2.3.15"

`dotnet restore` installs it as usual.

#### Configurable Connection

Since **Tres** is more-or-less a C#-to-F# conversion from **Dos**, we'll use the same `data-config.json` file, in the
root of the project:

    [lang=text]
    {
      "Hostname": "my-rethinkdb-server",
      "Database": "O2F3"
    }

We'll also add it to `project.json`, just below the list of files to compile:

    [lang=text]
    "copyToOutput": {
      "include": "data-config.json"
    }

Now, we'll create a file `Data.fs` to hold what we had in the `Data` directory in the prior two solutions.  It should
be added to the build order after `Entities.fs` and before `HomeModule.fs`.  We'll start this file off with our
`DataConfig` implementation:
*)
namespace Tres

open Newtonsoft.Json
open RethinkDb.Driver
open RethinkDb.Driver.Ast
open RethinkDb.Driver.Net

type DataConfig =
  { Hostname : string
    Port : int
    AuthKey : string
    Timeout : int
    Database : string
    }
with
  member this.CreateConnection () : IConnection =
    let bldr =
      seq<Connection.Builder -> Connection.Builder> {
        yield fun builder -> match this.Hostname with null -> builder | host -> builder.Hostname host
        yield fun builder -> match this.Port with 0 -> builder | port -> builder.Port port
        yield fun builder -> match this.AuthKey with null -> builder | key -> builder.AuthKey key
        yield fun builder -> match this.Database with null -> builder | db -> builder.Db db
        yield fun builder -> match this.Timeout with 0 -> builder | timeout -> builder.Timeout timeout
        }
      |> Seq.fold (fun builder block -> block builder) (RethinkDB.R.Connection())
    upcast bldr.Connect()
  static member FromJson json = JsonConvert.DeserializeObject<DataConfig> json
(**
This should be familiar at this point; we're using a record type instead of a class, and the `CreateConnection`
function utilizes the sequence style from **Dos**, just inlined as a _computation expression_ (more on those in a bit).
We also see `Seq.fold`, which takes the parameters in pretty much the opposite order of LINQ's `Aggregate`; instead of
`[collection].Aggregate([initial-state], [folder-func])`, it's `Seq.fold [folder-func] [initial-state] [collection]`
(which we're piping in with the `|>` operator).

The `upcast` is new.  Notice that `CreateConnection` is typed as `IConnection`; what's returned from the connection
builder is a `Connection`.  In most cases, F# requires an implementation to be explicitly cast to the interface it is
claiming to implement.  In our case, we want `IConnection` (vs. `IDisposable`, which it also implements).  There are 
two ways to do this; if the type can be inferred, as it can be here (because we've explicitly said what our return type
should be), you can use `upcast`.  Alternately, the last line of that function could read
`(bldr.Connect()) :> IConnection`.  _(I tend to prefer `upcast` when possible.)_

At this point, we need to take a detour through the land of asynchronous processing.  **Uno** and **Dos** both used
async/await to perform the RethinkDB calls, utilizing the `Task`-based async introduced in .NET 4.5.  F#'s approach to
asynchrony is different, but there are a few functions that provide the interoperability we need.  F# also uses an
`async` _computation expression_ to construct these.  The most important difference, for our purposes here, is that F#
`Async` instances are not automatically started the way a `Task` is in the C# world.

And, a quick detour from the detour - I promised there would be more on computation expressions.  These are expressions
that utilize an expression builder to declaratively create workflows within code.  They typically operate in a
specialized context; we've seen `seq`, we're about to see `async`, but there are many other uses as well.  Within a
computation expression, `let` and `do` have their same familiar behavior, and `return` returns a value; however,
`let!`, `do!`, and `return!` call into the builder to manipulate the specialized context.  A complete education in
computation expressions is outside of our scope; _F# for Fun and Profit_ has an
[excellent series on them](http://fsharpforfunandprofit.com/series/computation-expressions.html).

Before we see our first `async` computation expression, though, we need to make it be able to handle `Task`s as well as
F# async.  The code for `Extensions.fs` is below.  I won't delve too far into it at this point, but trust that what it
does is let us say `let! x = someTask` and it works just like it was F# async.  (This code is in the `AutoOpen`ed
module `Tres.Extensions`.)
*)
(*** hide ***)
[<AutoOpen>]
module ExampleExtensions =
(** *)
  open System.Threading.Tasks

  // H/T: Suave
  type AsyncBuilder with
    /// An extension method that overloads the standard 'Bind' of the 'async' builder. The new overload awaits on
    /// a standard .NET task
    member x.Bind(t : Task<'T>, f:'T -> Async<'R>) : Async<'R> = async.Bind (Async.AwaitTask t, f)

    /// An extension method that overloads the standard 'Bind' of the 'async' builder. The new overload awaits on
    /// a standard .NET task which does not commpute a value
    member x.Bind(t : Task, f : unit -> Async<'R>) : Async<'R> = async.Bind (Async.AwaitTask t, f)
    
    member x.ReturnFrom(t : Task<'T>) : Async<'T> = Async.AwaitTask t

(**
Add `Extensions.fs` to the build order at the top.  With those in place, we are now ready to return to `Data.fs` and
our startup code.  Before we move on, review
[the startup code from Dos](https://github.com/danieljsummers/FromObjectsToFunctions/tree/step-3/src/2-Nancy-CSharp/Data/EnvironmentExtensions.cs);
it has a main driver method at the top, with several methods below to perform each step.  Our F# code structure will be
somewhat inverted from this; as you generally cannot call forward into a source file, the "driver" code will be the
last code in the function, and the other code above it.

The `Table.cs` static class with constants is brought over as a module (below the data configuration code).  The
`RequireQualifiedAccess` attribute means that `Table` cannot be `open`ed; this prevents us from possibly providing an
unintended version of the identifier `Post` (for example).
*)
[<RequireQualifiedAccess>]
module Table =
  let Category = "Category"
  let Comment = "Comment"
  let Page = "Page"
  let Post = "Post"
  let User = "User"
  let WebLog = "WebLog"
(**
Extensions are defined differently in F#.  Whereas C# uses a static class and methods where the first parameter has
`this` before it, F# uses a module for the definitions (usually `AutoOpen`ed, so they're visible when the enclosing
namespace is opened), and a `type` declaration.  Here's the top of ours (below the table module):
*)
[<AutoOpen>]
module DataExtensions =
  type IConnection with
    member this.EstablishEnvironment database =
      // more to come...
(*** hide ***)
      let r = RethinkDB.R
(**
Rather than go through the entire file, let's just look at a representative example.  Here is the code to check for
table existence in C#:

    [lang=csharp]
    private static async Task CheckTables(this IConnection conn)
    {
        var existing = await R.TableList().RunResultAsync<List<string>>(conn);
        var tables = new List<string>
        {
            Table.Category, Table.Comment, Table.Page, Table.Post, Table.User, Table.WebLog
        };
        foreach (var table in tables)
        {
            if (!existing.Contains(table)) { await R.TableCreate(table).RunResultAsync(conn); }
        }
    }

Now, here's what it looks like in F#:
*)
      let checkTables () =
        async {
          let! existing = r.TableList().RunResultAsync<string list> this
          [ Table.Category; Table.Comment; Table.Page; Table.Post; Table.User; Table.WebLog ]
          |> List.filter (fun tbl -> not (existing |> List.contains tbl))
          |> List.map (fun tbl -> async { do! r.TableCreate(tbl).RunResultAsync this })
          |> List.iter Async.RunSynchronously
        }
(*** hide ***)
      let checkDatabase (db : string) = async { return () }
      let checkIndexes () = async { return () }
(**
The more interesting differences:

- In C#, `existing` is awaited; in F#, we use `let!` within the `async` computation expression to accomplish the same
thing.
- In C#, we defined a `List<string>` that we filled with our table names; in F#, we inlined a `string list`.
- In C#, we have a nice imperative loop that iterates over each table, checks to see whether it is in the list of
tables from the server, and creates it if it is not.  In F#, we declare that the list should be filtered to only names
not occurring in the list (`List.filter`); then that each of those names should be turned into an `Async` that will
create the table when it's run (`List.map`); then, that the list should be iterated, passing each item into
`Async.RunSynchronously` (`List.iter`).
- In C#, the return type of the method is `Task`; in F#, the type of `checkTables` is `Async<unit>`.
- When the C# `CheckTables` method call returns, the work has already been done.  When the F# `checkTables` function
returns, it returns an async workflow that is ready to be started.

The last 5 lines of the `EstablishEnvironment` extension method look like this:
*)
      async {
        do! checkDatabase database
        do! checkTables ()
        do! checkIndexes ()
      }
(*** hide ***)
open Microsoft.AspNetCore.Builder
open Nancy
open Nancy.Owin
open System.IO
(**
There are a few interesting observations here as well:

- `do!` is the equivalent of `let!`, except that we don't care about the result.
- As we saw above, `checkTables` returns an async workflow; yet, we're `do!`ing it in yet another async workflow; this
is perfectly acceptable.  If you've ever added async/await to a C# application, usually at a lower layer, and noticed
how async and await bubble up to the top layer - that's a similar concept to what we have here.
- `EstablishEnvironment`'s return type is `Async<unit>`.  It **still** hasn't run anything at this point; it has merely
assembled an asynchronous workflow that will do all of our environment checks once it is run.

#### Dependency Injection

We'll do the same thing we did for **Dos** - override `DefaultNancyBootstrapper` and register our connection there.
We'll do all of this in `App.fs`.  The first part, above the definition for `type Startup()`:
*)
type TresBootstrapper() =
  inherit DefaultNancyBootstrapper()
  
  override this.ConfigureApplicationContainer container =
    base.ConfigureApplicationContainer container
    let cfg = DataConfig.FromJson (File.ReadAllText "data-config.json")
    let conn = cfg.CreateConnection ()
    conn.EstablishEnvironment cfg.Database |> Async.RunSynchronously
    container.Register<IConnection> conn |> ignore
(**
Ah ha!  **There's** where we finally run our async workflow!  Now, again, we need to modify `Startup` (just below where
we put this code) to use this new bootstrapper.
*)
  member this.Configure (app : IApplicationBuilder) =
    app.UseOwin (fun x -> x.UseNancy (fun opt -> opt.Bootstrapper <- new TresBootstrapper()) |> ignore) |> ignore
(**
At this point, once `dotnet run` displays the "listening on port 5000" message, we should be able to look at
RethinkDB's `O2F3` database, tables, and indexes, just as we could for **Uno** and **Dos**.

[Back to Step 3](../step3)
*)