using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace iOne.Report.ReportTemplateSqlParameters
{
    public class ReportTemplateSqlParameterDto : EntityDto
    {
        public Guid SqlId { get; set; }
        public Guid ParameterId { get; set; }
    }
}
