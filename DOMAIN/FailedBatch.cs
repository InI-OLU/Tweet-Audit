using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tweet_Audit.DOMAIN
{
    public sealed class FailedBatch
    {
        public int BatchId { get; init; }

        public string Reason { get; init; } = string.Empty;

        public Tweet[] Tweets { get; init; } = [];
    }
}
