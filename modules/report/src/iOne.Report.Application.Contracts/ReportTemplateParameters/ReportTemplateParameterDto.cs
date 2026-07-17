using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace iOne.Report.ReportTemplateParameters
{
    public class ReportTemplateParameterDto : FullAuditedEntityDto<Guid>
    {
        public Guid ReportTemplateId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Status { get; set; } = "active";

        public string DataType { get; set; }

        public string? Description { get; set; }
    }
}
