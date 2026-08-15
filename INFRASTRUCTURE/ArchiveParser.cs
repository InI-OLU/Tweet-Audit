using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tweet_Audit.DOMAIN;

namespace Tweet_Audit.INFRASTRUCTURE
{
    public class ArchiveParser
    {
        private readonly ArchiveTweetPathSettings filepath;

        public ArchiveParser(IOptions<ArchiveTweetPathSettings> settings)
        {
            filepath = settings.Value;
        }
       
        public List<Tweet> ArchiveReadAndParse()
        {
            string rawContent = File.ReadAllText(filepath.ArchivePath);

            int jsonStartIndex = rawContent.IndexOf('[');
            string json = rawContent.Substring(jsonStartIndex);

            List<Tweet> tweets = JsonSerializer.Deserialize<List<Tweet>>(json)
                ?? new List<Tweet>();

            return tweets;
        }
    }
}
