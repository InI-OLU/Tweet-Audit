using Google.GenAI.Types;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tweet_Audit.DOMAIN;
using File = System.IO.File;

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
            if (!File.Exists(filepath.ArchivePath))
            {
                throw new FileNotFoundException($"File not found at {filepath.ArchivePath}");
            }
            string rawContent = File.ReadAllText(filepath.ArchivePath);
            int jsonStartIndex = rawContent.IndexOf('[');
            string json = rawContent.Substring(jsonStartIndex);

            List<TweetEnvelope> envelopes = JsonSerializer.Deserialize<List<TweetEnvelope>>(json)
                ?? new List<TweetEnvelope>();

            List<Tweet> tweets = envelopes.Select(e => e.Tweet).ToList();
            return tweets;
        }
    }
}
