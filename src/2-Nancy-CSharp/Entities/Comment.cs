namespace Dos.Entities
{

    public class Comment
    {
        public string Id { get; set; }

        public string PostId { get; set; }

        public string InReplyToId { get; set; }

        public string Name { get; set; }

        public string EmailAddress { get; set; }

        public string Url { get; set; }

        public string Status { get; set; }

        public long PostedOn { get; set; }

        public string Text { get; set; }
    }
}