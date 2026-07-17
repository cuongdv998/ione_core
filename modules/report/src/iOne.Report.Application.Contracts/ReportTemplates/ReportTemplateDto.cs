using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace iOne.Report.ReportTemplates
{
    public class ReportTemplateDto : FullAuditedEntityDto<Guid>
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public Guid DocumentId { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public DateTime? EffectDate { get; set; }

        public DateTime? ExpireDate { get; set; }
    }
}
