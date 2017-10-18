(*** hide ***)
#r "../../../packages/Chiron/lib/net40/Chiron.dll"
#r "../../../packages/Freya.Core/lib/net452/Freya.Core.dll"
#r "../../../packages/Freya.Machines.Http/lib/net452/Freya.Machines.Http.dll"
#r "../../../packages/Freya.Routers.Uri.Template/lib/net452/Freya.Routers.Uri.Template.dll"
#r "../../../packages/Freya.Types.Uri.Template/lib/net452/Freya.Types.Uri.Template.dll"
#r "../../../packages/Microsoft.AspNetCore.Hosting/lib/net451/Microsoft.AspNetCore.Hosting.dll"
#r "../../../packages/Microsoft.AspNetCore.Hosting.Abstractions/lib/net451/Microsoft.AspNetCore.Hosting.Abstractions.dll"
#r "../../../packages/Microsoft.AspNetCore.Http.Abstractions/lib/net451/Microsoft.AspNetCore.Http.Abstractions.dll"
#r "../../../packages/Microsoft.AspNetCore.Owin/lib/net451/Microsoft.AspNetCore.Owin.dll"
#r "../../../packages/Microsoft.AspNetCore.Server.Kestrel/lib/net451/Microsoft.AspNetCore.Server.Kestrel.dll"
#r "../../../packages/RethinkDb.Driver/lib/net45/RethinkDb.Driver.dll"

(**
### Quatro - Step 3

As with our previous versions, we'll start by adding the RethinkDB driver to `project.json`; we'll also bring the
`data-config.json` file from **Dos**/**Tres** into this project, changing the database name to `O2F4`. Follow the
[instructions for Tres](tres.html) up though the point where it says "we'll create a file `Data.fs`".

**Parsing `data.config`**

We'll use `Data.fs` in this project as well, but we'll do things a bit more functionally.  We'll use
[Chiron](https://xyncro.tech/chiron/) to parse the JSON file, and we'll set up a discriminated union (DU) for our
configuration parameters.

First, to be able to use Chiron, we'll need the package.  Add the following line within the `dependencies` section:

    [lang=text]
    "Chiron": "6.2.1",

Then, we'll start `Data.fs` with our DU.
*)
namespace Quatro

open Chiron
// other opens
type ConfigParameter =
  | Hostname of string
  | Port     of int
  | AuthKey  of string
  | Timeout  of int
  | Database of string
(*** hide ***)
open RethinkDb.Driver
open RethinkDb.Driver.Net
open System
open System.IO

// -- begin code lifted from #er demo --
(*** define: readerm-definition ***)
type ReaderM<'d, 'out> = 'd -> 'out
(*** hide ***)
module Reader =
(** *)
(*** define: run ***)
  let run dep (rm : ReaderM<_,_>) = rm dep
(** *)
(*** define: lift-dep ***)
  let liftDep (proj : 'd2 -> 'd1) (rm : ReaderM<'d1, 'output>) : ReaderM<'d2, 'output> = proj >> rm
(** *)
(*** hide ***)
open Reader
(**
This DU looks a bit different than the single-case DUs or enum-style DUs that
[we made in step 2](../step2/quatro.html).  This is a full-fledged DU with 5 different types, 3 strings and 2 integers.
The `DataConfig` record now becomes dead simple:
*)
type DataConfig = { Parameters : ConfigParameter list }
(**
We'll populate that using Chiron's `Json.parse` function.
*)
with
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
(*** define: database-property ***)
  member this.Database =
    match this.Parameters
          |> List.filter (fun x -> match x with Database _ -> true | _ -> false)
          |> List.tryHead with
    | Some (Database x) -> x
    | _ -> RethinkDBConstants.DefaultDbName

(**
There is a lot to learn in these lines.

* Before, if the JSON didn't parse, we raised an exception, but that was about it.  In this one, if the JSON doesn't
parse, we get a default connection.  Maybe this is better, maybe not, but it demonstrates that there is a way to handle
bad JSON other than an exception.
* `Object`, `String`, and `Number` are Chiron types (cases of a DU, actually), so our `match` statement uses the
destructuring form to "unwrap" the DU's inner value.  For `String`, `x` is a string, and for `Number`, `x` is a decimal
(that's why we run it through `int` to make our DUs.
* This version will raise an exception if we attempt to set an option that we do not recognize (something like
"databsae" - not that anyone I know would ever type it like that...).

Now, we'll adapt the `CreateConnection ()` function to read this new configuration representation:
*)
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
(**
Our folder function utilizes a `match` on our `ConfigParameter` DU.  Each time through, it **will** return a modified
version of the `builder` parameter, because one of them will match.  We then create our builder by folding the
parameter, using `R.Connection ()` as our beginning state, then return its `Connect ()` method.

For now, let's copy the rest of `Data.fs` from **Tres** to **Quatro** - this gives us the table constants and the
table/index initialization code.

**Dependency Injection: Functional Style**

One of the concepts that dependency injection is said to implement is "inversion of control;" rather than an object
compiling and linking a dependency at compile time, it compiles against an interface, and the concrete implementation
is provided at runtime.  (This is a bit of an oversimplification, but it's the basic gist.)  If you've ever done
non-DI/non-IoC work, and learned DI, you've adjusted your thinking from "what do I need" to "what will I need".  In the
functional world, this is done through a concept called the **`Reader` monad**.  The basic concept is as follows:

* We have a set of dependencies that we establish and set up in our code.
* We a process with a dependency that we want to be injected (in our example, our `IConnection` is one such
dependency).
* We construct a function that requires this dependency, and returns the result we seek.  Though we won't see it in
this step, it's easy to imagine a function that requires an `IConnection` and returns a `Post`.
* We create a function that, given our set of dependencies, will extract the one we need for this process.
* We run our dependencies through the extraction function, to the dependent function, which takes the dependency and
returns the result.

Confused yet?  Me too - let's look at code instead.  Let's create `Dependencies.fs` and add it to the build order above
`Entities.fs`.  This write-up won't expound on every line in this file, but we'll hit the highlights to see how all
this comes together.  `ReaderM` is a generic class, where the first type is the dependency we need, and the second type
is the type of our result.

After that (which will come back to in a bit), we'll create our dependencies, and a function to extract an
`IConnection` from it.
*)
type IDependencies =
  abstract Conn : IConnection

[<AutoOpen>]
module DependencyExtraction =

  let getConn (deps : IDependencies) = deps.Conn
(*** hide ***)
[<AutoOpen>]
module ExampleExtensions =
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
(** *)
(*** define: data-module ***)
[<RequireQualifiedAccess>]
module Data =
  let establishEnvironment database conn =
    let r = RethinkDB.R
    // etc.
(*** hide ***)
    let checkDatabase (db : string) = async { return () }
    let checkTables () = async { return () }
    let checkIndexes () =
      let indexesFor tbl = async { return! r.Table(tbl).IndexList().RunResultAsync<string list> conn }
      async { return () }
    async {
      do! checkDatabase database
      do! checkTables ()
      do! checkIndexes ()
    }

(**
Our `IDependencies` are pretty lean right now, but that's OK; we'll flesh it out in future steps.  We also wrote a
dead-easy function to get the connection; the signature is literally `IDependencies -> IConnection`.  No `ReaderM`
funkiness yet!

Now that we have a dependency "set" (of one), we need to go to `App.fs` and make sure we actually have a concrete
instance of this for runtime.  Add this just below the module declaration:
*)
(*** hide ***)
[<AutoOpen>]
module IConnectionExtensions =
  type IConnection with
    member this.EstablishEnvironment (database : string) = async { return () }
module App =
(** *)
  let lazyCfg = lazy (File.ReadAllText "data-config.json" |> DataConfig.FromJson)
  let cfg = lazyCfg.Force()
  let deps = {
    new IDependencies with
      member __.Conn
        with get () =
          let conn = lazy (cfg.CreateConnection ())
          conn.Force()
  }
(**
Here, we're using `lazy` to do this once-only-and-only-on-demand, then we turn around and pretty much demand it.  If
you're thinking this sounds a lot like singletons - your thinking is superb!  That's exactly what we're doing here.
We're also using F#'s inline interface declaration to create an implementation without creating a concrete class in
which it is held.

Maybe being our own IoC container isn't so bad!  Now, let's take a stab at actually connection, and running the
`EstablishEnvironment` function on startup.  At the top of `main`:
*)
(*** hide ***)
  let main _ =
(** *)
    let initDb (conn : IConnection) = conn.EstablishEnvironment cfg.Database |> Async.RunSynchronously 
    let start = liftDep getConn initDb
    start |> run deps
(*** define: better-init ***)
    liftDep getConn (Data.establishEnvironment cfg.Database >> Async.RunSynchronously)
    |> run deps
(*** define: composition-almost ***)
    let almost = Data.establishEnvironment cfg.Database
(*** define: composition-money ***)
    let money = Data.establishEnvironment cfg.Database >> Async.RunSynchronously
(*** hide ***)
    0
(**
But wait - we don't have a `Database` property on our data config; our configuration is just a list of
`ConfigParameter` selections.  No worries, though; we can expose a database property on it pretty easily.
*)

(*** include: database-property ***)

(**
OK - now our red squiggly lines are gone.  Now, if Jiminy Cricket had written F#, he would have told Pinocchio "Let the
types be your guide".  So, how are we doing with these?  `initDb` has the signature `IConnection -> unit`, `start` has
the signature `ReaderM<IDependencies, unit>`, and the third line is simply `unit`.  And, were we to run it, it would
work, but... it's not really composable.

Creating extension methods on objects works great in C#-land, and as we've seen, it works the same way in F#-land.
However, in the case where we want to write functions that expect an `IConnection` and return our expected result,
extension methods are not what we need.  Let's change our `AutoOpen`ed `DataExtensions` module to something like this:
*)
(*** include: data-module ***)
(**
Now, we have a function with the signature `string -> IConnection -> Async<unit>`.  This gets us close, but we still
have issues on either side of that signature.  On the front, if we were just hard-coding the database name, we could
drop the string parameter, and we'd have our `IConnection` as the first parameter.  On the return value, we will need
to run the `Async` workflow (remember, in F#, they're not started automatically); we need `unit`, not `Async<unit>`.

We'll use two key F# concepts to fix this up.  Currying (also known as partial application) allows us to look at every
return value that isn't the result as a function that's one step closer.  Looking at our signature above, you could
express it in English as "a function that takes a string, and returns a function that takes an `IConnection` and
returns an `Async` workflow."  So, to get a function that starts with an `IConnection`, we just provide the database
name.
*)
(*** include: composition-almost ***)
(**
The signature for `almost` is `IConnection -> Async<unit>`.  Just what we want.  For the latter, we use composition.
This is a word that can be used, for example, to describe the way the collection modules expect the collection as the
final parameter, allowing the output of one to be piped, using the `|>` operator, to the input of the next.  The other
is with the `>>` operator, which says to use the output of the first function as the input of the second function,
creating a single function from the two.  This is the one we'll use to run our `Async` workflow.
*)
(*** include: composition-money ***)
(**
The signature for `money` is now `IConnection -> unit`, just like we need.

Now, let's revisit `initDb` above.  Since we don't need the `IConnection` as a parameter, we can change that definition
to the same thing we have for `money` above.  And, since we don't need the parameter, we can just inline the call after
`getConn`; we'll just need to wrap the expression in parentheses to indicate that it's a function on its own.  And, we
don't need the definition of `start` anymore either - we can just pipe our entire expression into `run deps`.
*)
(*** include: better-init ***)
(**
It works!  We set up our dependencies, we composed a function using a dependency, and we used a `Reader` monad to make
it all work.  But, how did it work?  Given what we just learned above, let's look at the steps; we're coders, not
magicians.

First up, `liftDeps`.
*)
(*** include: lift-dep ***)
(**
The `proj` parameter is defined as a function that takes one value and returns another one.  The `rm` parameter is a
`Reader` monad that takes the **return** value of `proj`, and returns a `Reader` monad that takes the **parameter**
value of `proj` and returns an output type.  We passed `getConn` as the `proj` parameter, and its signature is
`IDependencies -> IConnection`; the second parameter was a function with the signature `IConnection -> unit`.  Where
does this turn into a `ReaderM`?  Why, the definition, of course!
*)
(*** include: readerm-definition ***)
(**
So, `liftDep` derived the expected `ReaderM` type from `getConn`; `'d1` is `IDependencies` and `'d2` is `IConnection`.
This means that the next parameter should be a function which takes an `IConnection` and returns the output of the type
we expect.  Since we pass in `IConnection -> unit`, `output` is `unit`.  When all is said and done, if we were to
assign a value to the top line, we would end up with `ReaderM<IDependencies, unit>`.

Now, to run it.  `run` is defined as:
*)
(*** include: run ***)
(**
This is way easier than what we've seen up to this point.  It takes an object and a `ReaderM`, and applies the object
to the first parameter of the monad.  By `|>`ing the `ReaderM<IDependencies, unit>` to it, and providing our
`IDependencies` instance, we receive the result; the reader has successfully encapsulated all the functions below it.
From this point on, we'll just make sure our types are correct, and we'll be able to utilize not only an `IConnection`
for data manipulation, but any other dependencies we may need to define.

Take a deep breath.  Step 3 is done, and not only does it work, we understand why it works.   

[Back to Step 3](../step3)
*)