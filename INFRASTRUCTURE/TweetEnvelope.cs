using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tweet_Audit.DOMAIN;

namespace Tweet_Audit.INFRASTRUCTURE
{
    public class TweetEnvelope
    {
        [JsonPropertyName("tweet")]
        public Tweet Tweet { get; set; }
    }
}
