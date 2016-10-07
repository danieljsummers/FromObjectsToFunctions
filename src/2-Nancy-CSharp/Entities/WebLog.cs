namespace Dos.Entities
{
    using Newtonsoft.Json;

    public class WebLog
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Subtitle { get; set; }

        public string DefaultPage { get; set; }

        public string ThemePath { get; set; }

        public string UrlBase { get; set; }

        public string TimeZone { get; set; }
    }
}