using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Reports
{
    [Table("report_template")]
    public class ReportTemplate : FullAuditedAggregateRoot<Guid>
    {
        [StringLength(150)]
        public virtual string? Name { get; private set; }

        [Required]
        [StringLength(100)]
        public virtual string Code { get; private set; }

        [Required]
        public virtual Guid DocumentId { get; private set; }

        public virtual string? Description { get; private set; }

        [Required]
        [StringLength(20)]
        public virtual string Status { get; private set; } = "active";

        public virtual DateTime? EffectDate { get; private set; }

        public virtual DateTime? ExpireDate { get; private set; }
    }
}
