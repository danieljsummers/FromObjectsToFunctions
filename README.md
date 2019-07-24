# objects () |> functions

**UPDATE (July 2019)** - Development of version 2 is happening in a different repository; version 2 inserts a Giraffe solution as Quatro, and moves Freya to Cinco.

Repository: https://github.com/bit-badger/o2f

Website: https://objects-to-functions.bitbadger.solutions

## What

This repository will track the development of a rudimentary multi-site blog platform, in parallel, in 4 different
environments:

1. ASP.NET Core MVC / C# ("**Uno**")

2. Nancy / C# ("**Dos**")

3. Nancy / F# ("**Tres**")

4. Freya / F# ("**Quatro**")

The goal is to be able to start any of the four solutions, and be able to use the same data store and have the behavior
of each site work the same.  All four will use RethinkDB to persist the data (and, where required, for session storage
as well).

## Why

The idea for this came out of a chat in the F# community Slack.  Lighter weight frameworks can provide real benefits,
and a more composable system can be easier to reason about and maintain.  However, when one goes "full functional",
there are concepts that do not even directly translate.  Compound this with the language of "monads" and "applicative
functors" and the like, and an OO person's eyes can start to glaze over.

## Who (and continuing with Why)

I am that developer.  I've admired F# for years now, and almost have step 3 coded in another repository; I'd probably
be live with it now if I hadn't decided to use it as a .NET Core test.  I'm learning this as I go along, and I'm
grateful for the support of the F# community as I participate.  To help amplify their efforts, and to demonstrate how
to get not just from C# to F#, but from object thinking to functional thinking, this is where I'll learn publicly.

_This learning is not my primary occupation, so the pace may be slow; my hope is that the result will be worth the wait._

## The Steps

The plan is laid out, and will be documented as we go along, on
[GitHub Pages](https://danieljsummers.github.io/FromObjectsToFunctions/).
