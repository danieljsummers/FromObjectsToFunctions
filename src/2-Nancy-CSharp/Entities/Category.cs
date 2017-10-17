namespace Dos.Entities
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class Category
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string WebLogId { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public string ParentId { get; set; }

        public ICollection<string> Children { get; set; } = new List<string>();
    }
}