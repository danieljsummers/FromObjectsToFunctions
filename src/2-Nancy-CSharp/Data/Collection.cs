using System;

namespace Dos.Data
{
    public static class Collection
    {
        public const string Category = "Categories";

        public const string Comment = "Comments";

        public const string Page = "Pages";

        public const string Post = "Posts";

        public const string User = "Users";

        public const string WebLog = "WebLogs";

        public static string IdFor(string collection, Guid id) => $"{collection}/{id.ToString()}";

        public static (string Collection, Guid Id) FromId(string documentId)
        {
            try
            {
                var parts = (documentId ?? "").Split('/');
                return parts.Length == 2 ? (parts[0], Guid.Parse(parts[1])) : ("", Guid.Empty);
            }
            catch (FormatException)
            {
                return ("", Guid.Empty);
            }
        }
    }
}