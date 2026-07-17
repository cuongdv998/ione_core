using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProCoverageGroups;

public interface IProCoverageGroupAppService : ICrudAppService<
    ProCoverageGroupDto,
    Guid,
    GetProCoverageGroupsInput,
    CreateProCoverageGroupDto,
    UpdateProCoverageGroupDto>
{
}

