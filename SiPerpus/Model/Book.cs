using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiPerpus.Model
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
