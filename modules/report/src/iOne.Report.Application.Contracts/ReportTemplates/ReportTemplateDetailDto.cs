using iOne.Master.ResDocuments;
using iOne.Report.ReportTemplateParameters;
using iOne.Report.ReportTemplateSqls;
using System;
using System.Collections.Generic;



namespace iOne.Report.ReportTemplates
{
    public class ReportTemplateDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Code { get; set; }

        public Guid DocumentId { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public DateTime? EffectDate { get; set; }

        public DateTime? ExpireDate { get; set; }

        public FileResponseDto Document { get; set; }
        public List<ReportTemplateParameterDto> Parameters { get; set; }
        public List<ReportTemplateSqlDetailDto> Sqls { get; set; }
    }
}
