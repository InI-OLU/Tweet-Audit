using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tweet_Audit.DOMAIN.Exceptions
{
    public class MalformedGeminiResponseException:Exception
    {
        public MalformedGeminiResponseException():base("Gemini gave a malformed response")
        {

        }
    }
}
