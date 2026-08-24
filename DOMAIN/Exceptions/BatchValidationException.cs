using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tweet_Audit.DOMAIN.Exceptions
{
    public class BatchValidationException:Exception
    {
        public BatchValidationException() : base("Batch Validation Failed")
        {

        }
    }
}
