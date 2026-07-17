using System;
using System.Collections.Generic;

namespace iOne.Report.ReportTemplates
{
    public class ReportExportResultDto
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}
