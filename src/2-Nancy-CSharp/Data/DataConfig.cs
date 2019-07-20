using Newtonsoft.Json;

namespace Dos.Data
{
    public class DataConfig
    {
        public string Url { get; set; }
        public string Database { get; set; }

        [JsonIgnore]
        public string[] Urls => new[] { Url };
    }
}
