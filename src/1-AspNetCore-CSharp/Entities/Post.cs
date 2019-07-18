namespace Uno.Entities
{
    using System.Collections.Generic;

    public class Post
    {
        public string Id { get; set; }

        public string WebLogId { get; set; }

        public string AuthorId { get; set; }

        public string Status { get; set; }

        public string Title { get; set; }

        public string Permalink { get; set; }

        public long PostedOn { get; set; }

        public long UpdatedOn { get; set; }

        public string Text { get; set; }

        public ICollection<string> CategoryIds { get; set; } = new List<string>();

        public ICollection<string> Tags { get; set; } = new List<string>();

        public ICollection<Revision> Revisions { get; set; } = new List<Revision>();
    }
}