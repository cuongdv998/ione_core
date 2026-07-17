using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iOne.Report.ReportTemplateSqls
{
    public class CreateReportTemplateSqlDto
    {
        [Required]
        public Guid ReportTemplateId { get; set; }

        [Required]
        public string SqlText { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string VarName { get; set; } = null!;

        public string Status { get; set; } = "active";

        [Required]
        public bool IsSingle { get; set; } = false;

        public List<string> ParameterCodes { get; set; } = new();
    }
}
