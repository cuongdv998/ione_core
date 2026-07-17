using iOne.Report.ReportTemplateParameters;
using iOne.Report.ReportTemplateSqls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iOne.Report.ReportTemplates
{
    public class UpdateReportTemplateDto
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Code { get; set; }

        [Required]
        public Guid DocumentId { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Trạng thái: active | deactive
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "active";
        public DateTime? EffectDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        // Use input DTOs that allow nullable Id so client can omit id for new items
        public List<UpdateReportTemplateParameterInputDto> Parameters { get; set; } = new();
        public List<ReportTemplateSqlInputDto> Sqls { get; set; } = new();
    }
}
