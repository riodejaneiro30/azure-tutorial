using Newtonsoft.Json;
using Nexus.Base.CosmosDBRepository;

namespace SiPerpus.DAL.Models
{
    public class BookNexus : ModelBase
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
    }
}
