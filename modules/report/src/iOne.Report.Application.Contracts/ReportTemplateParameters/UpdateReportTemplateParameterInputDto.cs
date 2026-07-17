using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Report.ReportTemplateParameters
{
    public class UpdateReportTemplateParameterInputDto
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid ReportTemplateId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        [Required]
        [StringLength(10)]
        public string DataType { get; set; }

        public string Status { get; set; } = "active";

        [StringLength(250)]
        public string? Description { get; set; }
    }
}
