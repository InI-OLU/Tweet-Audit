using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweet_Audit.DOMAIN;

namespace Tweet_Audit.APPLICATION
{
    public class TweetUrlBuilder
    {
        private readonly UserName _options;

        public TweetUrlBuilder(IOptions<UserName> options)
        {
            _options = options.Value;
        }
        public List<string> UrlBuilder(List<TweetVerdict> flaggedVerdicts)
        {
            var Url = new List<string>();
            foreach(var verdict in flaggedVerdicts)
            {
                var url = $"https://x.com/{_options.Name}/status/{verdict.Id}";
             Url.Add(url);
            }
            return Url;

        }
    }
}
