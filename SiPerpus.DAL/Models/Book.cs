using Newtonsoft.Json;

namespace SiPerpus.DAL.Models
{
    public class Book
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }
    }
}
