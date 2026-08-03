using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tweet_Audit.DOMAIN
{
    public class AlignmentCriteria
    {
        public List<string>? ForbiddenWords { get; set; }
        public bool? ProfessionalCheck { get; set; }
        public string? Tone { get; set; }
        public bool? ExcludePolitics { get; set; }

    }
}
