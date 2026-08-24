using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tweet_Audit.DOMAIN
{
    public class CsvOutput
    {
        [Name("tweet_url")]
        [Index(0)]
        public required string TweetUrl { get; set; }
        [Name("deleted")]
        [Index(1)]
        public bool DeleteStatus { get; set; } 
    }
}
