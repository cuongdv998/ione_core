using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Reports
{
    [Table("report_template_sql")]
    public class ReportTemplateSql : FullAuditedAggregateRoot<Guid>
    {
        [Required]
        public virtual Guid ReportTemplateId { get; private set; }

        [Required]
        public virtual string SqlText { get; private set; }

        [Required]
        [StringLength(50)]
        public virtual string VarName { get; private set; }

        [Required]
        [StringLength(15)]
        public virtual string Status { get; private set; } = "active";

        [Required]
        public virtual bool IsSingle { get; private set; } = false;

        // Navigation
        public virtual ReportTemplate? ReportTemplate { get; protected set; }
    }
}
