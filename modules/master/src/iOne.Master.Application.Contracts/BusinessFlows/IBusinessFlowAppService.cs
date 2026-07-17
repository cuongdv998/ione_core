using System;
using Volo.Abp.Application.Services;

namespace iOne.Master.BusinessFlows;

public interface IBusinessFlowAppService : ICrudAppService<
    BusinessFlowDto,
    Guid,
    GetBusinessFlowsInput,
    CreateBusinessFlowDto,
    UpdateBusinessFlowDto>
{
}
