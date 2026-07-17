using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace iOne.Report.ReportTemplateSqls
{
    public class ReportTemplateSqlDto : FullAuditedEntityDto<Guid>
    {
        public Guid ReportTemplateId { get; set; }

        public string SqlText { get; set; } = null!;

        public string VarName { get; set; } = null!;

        public string Status { get; set; } = "active";
        public bool IsSingle { get; set; } = false;
    }
}
