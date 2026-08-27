using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tweet_Audit.APPLICATION.INTERFACE
{
    public interface IGeminiClient
    {
        Task<string> GeminiAuditAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
