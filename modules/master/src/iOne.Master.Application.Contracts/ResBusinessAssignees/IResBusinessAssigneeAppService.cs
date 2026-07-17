using System;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResBusinessAssignees;

public interface IResBusinessAssigneeAppService : ICrudAppService<
    ResBusinessAssigneeDto,
    Guid,
    GetResBusinessAssigneesInput,
    CreateResBusinessAssigneeDto,
    UpdateResBusinessAssigneeDto>
{
}
