### Data Model

_(Feel free to browse
[the checkpoint for step 2](https://github.com/danieljsummers/FromObjectsToFunctions/tree/v2-step-2) as you follow
along.)_

#### Overview

For our data model, we will begin with how we will store it.  At a high level:

- Web logs have a name, an optional subtitle, a theme, a URL, and a time zone
- Users have an e-mail address, a password, a first name, a last name, a preferred name, and a personal URL
- Categories have a name, a URL-friendly "slug", and a description
- Posts have a title, a status, a permalink, when they were published and last updated, 0 or more tags, the text of the post, and a list of revisions of that post
- Pages have a title, a permalink, when they were published and last updated, whether they should show in the default page list (think "About", "Contact", etc.), the text of the page, and a list of revisions to that page
- Comments have a name, an e-mail address, an optional URL, a status, when they were posted, and the text of the comment

As far as relationships among these entities:

- Users can have differing authorization levels among the different web logs to which they are authorized
- Categories, Posts, and Pages all each belong to a specific web log
- Comments belong to a specific Post
- Posts are linked to the user who authored them
- Categories can be nested (parent/child)
- Comments can be marked as replies to another comment
- Posts can be assigned to multiple Categories (and can have multiple Comments, as implied above)
- Revisions (Posts and Pages) will track the date/time of the revision and the text of the post or page as of that time

Both Uno and Dos will use the same C# model. For Tres, we'll convert classes to F# record types (and `null` checks to
`Option`s). For Quatro, we'll make some concrete types for some of these primitives, making it more difficult to
represent an invalid state within our model. (We'll also deal with the implications of those in step 3.)

#### Implementation Notes

Our C# data model looks very much like one you'd see in an Entity Framework project. The major difference is that what
would be the navigation properties; collections (ex. the `Revisions` collection in the `Page` and `Post`) are part of
the type, rather than a `Revision` being its own entity, while parent navigation properties (ex. `WebLog` for entities
that define a `WebLogId` property) do not exist. Even if you are unfamiliar with Entity Framework, you will likely
easily see how this model could be represented in a relational database.

Some other design decisions:

- We will use strings (created from `Guid`s) as our Ids for entities, and all of documents will have `Id` as the property _(this supports the convention RavenDB uses to identify document identifiers)_
- Authorization levels, post statuses, and comment statuses are represented as strings, but we provide a means to avoid magic strings in the code while dealing with these
- Properties representing date/time will be stored as `long`/`int64`, representing ticks. _(We'll use NodaTime for manipulation, but this would also support using something built-in like `DateTime.UtcNow.Ticks`.)_
- While you generally want to properly comment all classes and public properties, we will exclude these for brevity's sake.

#### Project-Specific Notes

**Uno / Dos** - [In Depth](uno-dos.html)

**Tres** - [In Depth](tres.html)

**Quatro** - [In Depth](quatro.html)