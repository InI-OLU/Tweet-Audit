using System.Text.Json.Serialization;

namespace Tweet_Audit.DOMAIN
{
    public class Tweet
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("full_text")]
        public string FullText { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

    }
}