using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace iOne.Report.ReportTemplates
{
    public class FilterDto : PagedAndSortedResultRequestDto
    {
        public string? code { get; set; }
        public string? name { get; set; }
        public string? status { get; set; }
    }
}
