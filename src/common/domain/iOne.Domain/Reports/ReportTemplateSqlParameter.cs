using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace iOne.Reports
{
    public class ReportTemplateSqlParameter : Entity
    {
        public Guid SqlId { get; set; }

        public Guid ParameterId { get; set; }
        protected ReportTemplateSqlParameter() { }
        public ReportTemplateSqlParameter(Guid sqlId, Guid parameterId)
        {
            SqlId = sqlId;
            ParameterId = parameterId;
        }
        public override object[] GetKeys()
        => [SqlId, ParameterId];
    }
}
