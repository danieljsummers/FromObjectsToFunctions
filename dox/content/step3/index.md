## RethinkDB Connection

Database connections are generally defined at either the application or request instance levels.  The
[C# RethinkDB driver](https://github.com/bchavez/RethinkDb.Driver) is designed for the former, and configuring it as a
singleton is the recommended implementation.  We will do that for our application.  In the process of ensuring that we
can properly configure this instance, we will also have to address the concepts of configuration and dependency
injection (or, in the case of our Freya implementation, its replacement).  You can review the all the code at
[the checkpoint for step 3](https://github.com/danieljsummers/FromObjectsToFunctions/tree/step-3).


### A Bit about RethinkDB

RethinkDB is a document database, and the C# implementation allows us to represent our documents as Plain Old CLR
Objects (AKA POCOs).  It uses JSON.Net to serialize POCOs into JSON documents, which are then stored by the server.
It exposes collections of documents as "databases" and "tables" within each database, mirroring its relational database
cousins.  However, there are no schemas for tables, and a table can have documents of varying formats.  Each table can
have one or more indexes that can be used to retrieve documents without scanning the entire table.  It provides its own
query language (ReQL) that utilizes a fluent interface, where queries begin with an `R.` or `r.` and end in a `Run*`
statement.

It has other features, such as server clustering and change feeds, but these will not be part of our project (although
change feeds could be an interesting Step n-2 project for post comments).  We will use a single instance of RethinkDB,
and a single database within it, for all our data.

### All Implementations

Each of our implementations will allow for the following user-configurable options:

- `Hostname` - The hostname for the server; defaults to "localhost"
- `Port` - The port to use to connect to the server; defaults to 28015
- `Database` - The default database to use for queries that do not specify a database; defaults to "test"
- `AuthKey` - The authorization key to provide for the connection; defaults to "" (empty)
- `Timeout` - How long to wait when connecting; defaults to 20

Additionally, we will write start-up code that ensures our requisite tables exist, and that each of these tables has
the appropriate indexes.  To hold our documents, we will create the following tables:

- Category
- Comment
- Page
- Post
- User
- WebLog

We will be able to retrieve individual documents by Id without any further definition.  Additionally, we will create
indexes to support the following scenarios:

- Category retrieval by web log Id _(used to generate lists of available categories)_
- Category retrieval by web log Id and slug _(used to retrieve details for category archive pages)_
- Comment retrieval by post Id _(used to retrieve comments for a post)_
- Page retrieval by web log Id _(used to retrieve a list of pages)_
- Page retrieval by permalink _(used to fulfill single page requests)_
- Post retrieval by web log Id _(used many places)_
- Post retrieval by category Id _(used to get posts for category archive pages)_
- Post retrieval by tag _(used to get posts for tag archive pages)_
- User retrieval by e-mail address and password _(used for log in)_

### Individual Implementations

Each of these not only evolves from step 2 to step 3, they also evolve as Uno moves to Quatro.  It may help
understanding to read each of them, even if your interest is just in one of them.

**Uno** - [In Depth](uno.html)

**Dos** - [In Depth](dos.html)

**Tres** - [In Depth](tres.html)

**Quatro** - [In Depth](quatro.html)