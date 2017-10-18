namespace Uno.Data
{
    using RethinkDb.Driver;
    using RethinkDb.Driver.Net;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using static RethinkDb.Driver.RethinkDB;

    public static class EnvironmentExtensions
    {
        public static async Task EstablishEnvironment(this IConnection conn, string database)
        {
            await conn.CheckDatabase(database);
            await conn.CheckTables();
            await conn.CheckIndexes();
        }

        private static async Task CheckDatabase(this IConnection conn, string database)
        {
            if (!string.IsNullOrEmpty(database) && !R.DbList().RunResult<List<string>>(conn).Contains(database))
            {
                await R.DbCreate(database).RunResultAsync(conn);
            }
        }

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

        private static async Task CheckIndexes(this IConnection conn)
        {
            await conn.CheckCategoryIndexes();
            await conn.CheckCommentIndexes();
            await conn.CheckPageIndexes();
            await conn.CheckPostIndexes();
            await conn.CheckUserIndexes();
        }

        private static async Task CheckCategoryIndexes(this IConnection conn)
        {
            var indexes = await conn.IndexesFor(Table.Category);
            if (!indexes.Contains("WebLogId"))
            {
                await R.Table(Table.Category).IndexCreate("WebLogId").RunResultAsync(conn);
            }
            if (!indexes.Contains("WebLogAndSlug"))
            {
                await R.Table(Table.Category)
                    .IndexCreate("WebLogAndSlug", row => R.Array(row["WebLogId"], row["Slug"]))
                    .RunResultAsync(conn);
            }
        }

        private static async Task CheckCommentIndexes(this IConnection conn)
        {
            if (!(await conn.IndexesFor(Table.Comment)).Contains("PostId"))
            {
                await R.Table(Table.Comment).IndexCreate("PostId").RunResultAsync(conn);
            }
        }

        private static async Task CheckPageIndexes(this IConnection conn)
        {
            var indexes = await conn.IndexesFor(Table.Page);
            if (!indexes.Contains("WebLogId"))
            {
                await R.Table(Table.Page).IndexCreate("WebLogId").RunResultAsync(conn);
            }
            if (!indexes.Contains("WebLogAndPermalink"))
            {
                await R.Table(Table.Page)
                    .IndexCreate("WebLogAndPermalink", row => R.Array(row["WebLogId"], row["Permalink"]))
                    .RunResultAsync(conn);
            }
        }

        private static async Task CheckPostIndexes(this IConnection conn)
        {
            var indexes = await conn.IndexesFor(Table.Post);
            if (!indexes.Contains("WebLogId"))
            {
                await R.Table(Table.Post).IndexCreate("WebLogId").RunResultAsync(conn);
            }
            if (!indexes.Contains("Tags"))
            {
                await R.Table(Table.Post).IndexCreate("Tags").OptArg("multi", true).RunResultAsync(conn);
            }
        }

        private static async Task CheckUserIndexes(this IConnection conn)
        {
            if (!(await conn.IndexesFor(Table.User)).Contains("EmailAddress"))
            {
                await R.Table(Table.User).IndexCreate("EmailAddress").RunResultAsync(conn);
            }
        }

        private static Task<List<string>> IndexesFor(this IConnection conn, string table) =>
            R.Table(table).IndexList().RunResultAsync<List<string>>(conn);
    }
}