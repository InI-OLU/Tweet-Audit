using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tweet_Audit.DOMAIN
{
    public class Tweet
    {
        public int Id { get; set; }
        public string FullText { get; set; }
        public string CreatedAt { get; set; }
        public string RetweetCount { get; set; }
        public string FavoriteCount { get; set; }
    }
}
