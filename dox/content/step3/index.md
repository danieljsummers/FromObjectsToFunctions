## RavenDB Connection

Database connections are generally defined at either the application or request instance levels.  The [RavenDB client](https://github.com/ravendb/ravendb/tree/v4.2/src/Raven.Client) is designed for the former, and configuring its `IDocumentStore` type as a singleton is the recommended implementation, which we will do for our applications. In the process of ensuring that we can properly configure this instance, we will also have to address the concepts of configuration and dependency injection (or, in the case of our Freya implementation, its replacement). You can review the all the code at [the checkpoint for step 3](https://github.com/danieljsummers/FromObjectsToFunctions/tree/v2-step-3).

### A Bit about RavenDB

[RavenDB](https://ravendb.net/) is a document database written in C# that allows the us to store our Plain Old CLR Objects (AKA POCOs) as documents. It uses JSON.Net to serialize POCOs into JSON documents, which are then stored by the server. It stores data in "databases", with documents grouped together in "collections" based on their Id. There are no schemas for these documents, or within a collection; however, in practice, most documents in a collection will have a similar shape. Documents can be indexed, to allow fast retrieval or even alternate forms of the data. It provides its own query language (RQL), based on Lucene, and also exposes most all its features in LINQ syntax off the document collection.

It also supports multi-node clusters, and a lot of advanced features we won't be tapping for this project. We will use a single instance of RavenDB, and a single database within it, for all our data. RavenDB will run for seven days without a license installed, but they offer free developer and community licenses at their site.

### All Implementations

Each of our implementations will allow for the following user-configurable options:

- `Url` - The URL and port of the RavenDB server
- `Database` - The database to use for queries, so that we do not need to specify one for each call

_(If you decide to go live with this, you'll end up generating a client certificate and at least a community license; we'll address this at the end of the project.)_

Additionally, we will write start-up code that ensures our document collections are properly indexed. A collection does not have to "exist" before defining an index, so we can create the indexes on a database without any documents. We will create the appropriate database through the Raven Studio rather than through code, so that we can set it as the default when we initialize our `IDocumentStore` instance. We do not need to create indexes to be able to retrieve individual documents by Id; we will, though, need to address the following scenarios:

- Category retrieval by web log Id _(used to generate lists of available categories)_
- Category retrieval by web log Id and slug _(used to retrieve details for category archive pages)_
- Comment retrieval by post Id _(used to retrieve comments for a post)_
- Page retrieval by web log Id _(used to retrieve a list of pages)_
- Page retrieval by web log Id and permalink _(used to fulfill single page requests)_
- Post retrieval by web log Id _(used many places)_
- Post retrieval by web log Id and permalink _(used to fufill single post requests)_
- Post retrieval by web log Id and category Id _(used to get posts for category archive pages)_
- Post retrieval by web log Id and tag _(used to get posts for tag archive pages)_
- User retrieval by e-mail address and password _(used for log in)_

One interesting feature that distinguishes RavenDB indexes from relational indexes is that if you index multiple fields, you can utilize the index for any of those fields, not just the first one listed. This means that we will only need to create one index on the `Category` collection, for example, because we have one requirement by web log Id, and one by web log Id and slug. A single index on both fields will support both queries.

### Individual Implementations

Each of these not only evolves from step 2 to step 3, they also evolve as Uno moves to Quatro.  It may help understanding to read each of them, even if your interest is just in one of them.

**Uno** - [In Depth](uno.html)

**Dos** - [In Depth](dos.html)

**Tres** - [In Depth](tres.html)

**Quatro** - [In Depth](quatro.html)