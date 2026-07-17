using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResPartnerTypes;

public interface IResPartnerTypeAppService : ICrudAppService<
    ResPartnerTypeDto,
    Guid,
    GetResPartnerTypesInput,
    CreateResPartnerTypeDto,
    UpdateResPartnerTypeDto>
{
}

