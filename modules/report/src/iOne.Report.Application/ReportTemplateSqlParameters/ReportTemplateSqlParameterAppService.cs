using iOne.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Report.ReportTemplateSqlParameters
{
    public class ReportTemplateSqlParameterAppService : ApplicationService, IReportTemplateSqlParameterAppService
    {
        private readonly IRepository<ReportTemplateSqlParameter> _repository;

        public ReportTemplateSqlParameterAppService(
            IRepository<ReportTemplateSqlParameter> repository)
        {
            _repository = repository;
        }


        public async Task<ReportTemplateSqlParameterDto> CreateAsync(
           CreateUpdateReportTemplateSqlParameterDto
            input)
        {
            var exists = await _repository.AnyAsync(x =>
                x.SqlId == input.SqlId &&
                x.ParameterId == input.ParameterId);

            if (exists)
            {
                throw new BusinessException("ReportTemplateSqlParameter.AlreadyExists");
            }

            var entity = new ReportTemplateSqlParameter(
                input.SqlId,
                input.ParameterId
            );

            await _repository.InsertAsync(entity, autoSave: true);

            return new ReportTemplateSqlParameterDto
            {
                SqlId = entity.SqlId,
                ParameterId = entity.ParameterId
            };
        }
    }
}
