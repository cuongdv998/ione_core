using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResReasonGroups;

public interface IResReasonGroupAppService : ICrudAppService<
    ResReasonGroupDto,
    Guid,
    GetResReasonGroupsInput,
    CreateResReasonGroupDto,
    UpdateResReasonGroupDto>
{
}
