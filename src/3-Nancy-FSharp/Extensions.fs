[<AutoOpen>]
module Tres.Extensions

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
