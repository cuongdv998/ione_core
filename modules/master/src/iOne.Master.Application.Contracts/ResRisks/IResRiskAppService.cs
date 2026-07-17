using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResRisks;

public interface IResRiskAppService : ICrudAppService<
    ResRiskDto,
    Guid,
    GetResRisksInput,
    CreateResRiskDto,
    UpdateResRiskDto>
{
}

