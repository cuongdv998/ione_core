using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.Reports
{
    public class ReportTemplateParameter : FullAuditedAggregateRoot<Guid>
    {
        [Required]
        public virtual Guid ReportTemplateId { get; private set; }

        [Required]
        [StringLength(50)]
        public virtual string Code { get; private set; }

        [Required]
        [StringLength(250)]
        public virtual string Name { get; private set; }

        [Required]
        [StringLength(15)]
        public virtual string Status { get; private set; } = "active";

        [StringLength(250)]
        public virtual string? Description { get; private set; }

        [Required]
        [StringLength(10)]
        public virtual string DataType { get; private set; }

        // Navigation
        public virtual ReportTemplate? ReportTemplate { get; protected set; }
    }
}
