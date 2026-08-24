using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tweet_Audit.DOMAIN
{
    public sealed class BatchContext
    {
        public int BatchId { get; init; }

        public Tweet[] Tweets { get; init; } = [];
    }
}
