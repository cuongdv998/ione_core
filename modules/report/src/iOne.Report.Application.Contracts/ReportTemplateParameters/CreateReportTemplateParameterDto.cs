using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iOne.Report.ReportTemplateParameters
{
    public class CreateReportTemplateParameterDto
    {
        [Required]
        public Guid ReportTemplateId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        [Required]
        [StringLength(10)]
        public string DataType { get; set; }

        public string Status { get; set; } = "active";

        [StringLength(250)]
        public string? Description { get; set; }
    }
}
