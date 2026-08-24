using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tweet_Audit.DOMAIN
{
    public class BatchResult
    {
        public List<TweetVerdict>? Batch { get; set; }
        public bool IsFailed { get; set; }
    }
}
