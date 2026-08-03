using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tweet_Audit.DOMAIN;

namespace Tweet_Audit.INFRASTRUCTURE
{
    public class ArchiveParser
    {

       
        public List<Tweet> ArchiveReadAndParse()
        {
            using FileStream tweetFile = File.OpenRead();
            jsonD
            List<Tweet> tweets = JsonSerializer.Deserialize<List<Tweet>>(json);
            return tweets;
        }
    }
}
