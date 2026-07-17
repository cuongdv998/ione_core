using System;
using System.Collections.Generic;

namespace iOne.Report.ReportTemplateSqls
{
    public class ReportTemplateSqlDetailDto
    {
        public Guid Id { get; set; }
        public Guid ReportTemplateId { get; set; }
        public string SqlText { get; set; } = null!;
        public string VarName { get; set; } = null!;
        public string Status { get; set; } = "active";
        public bool IsSingle { get; set; } = false;
        public List<string> ParameterCodes { get; set; } = new();
    }
}
