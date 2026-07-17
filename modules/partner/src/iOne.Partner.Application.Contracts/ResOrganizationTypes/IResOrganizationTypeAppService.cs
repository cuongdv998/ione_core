using System;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResOrganizationTypes;

public interface IResOrganizationTypeAppService : ICrudAppService<
    ResOrganizationTypeDto,
    Guid,
    GetResOrganizationTypesInput,
    CreateResOrganizationTypeDto,
    UpdateResOrganizationTypeDto>
{
}

