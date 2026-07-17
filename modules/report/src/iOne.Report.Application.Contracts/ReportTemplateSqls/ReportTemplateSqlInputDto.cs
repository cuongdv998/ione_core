using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Report.ReportTemplateSqls
{
    public class ReportTemplateSqlInputDto
    {
        public Guid? Id { get; set; }

        [Required]
        public string SqlText { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string VarName { get; set; } = null!;

        public string Status { get; set; } = "active";

        public bool IsSingle { get; set; } = false;

        public List<string> ParameterCodes { get; set; } = new();

        public Guid ReportTemplateId { get; set; }
    }
}
