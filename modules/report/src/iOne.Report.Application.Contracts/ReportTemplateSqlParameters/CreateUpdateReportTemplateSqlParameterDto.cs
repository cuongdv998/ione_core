using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iOne.Report.ReportTemplateSqlParameters
{
    public class CreateUpdateReportTemplateSqlParameterDto
    {
        [Required]
        public Guid SqlId { get; set; }

        [Required]
        public Guid ParameterId { get; set; }
    }
}
