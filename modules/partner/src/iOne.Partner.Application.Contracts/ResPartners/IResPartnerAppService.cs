using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResPartners;

public interface IResPartnerAppService : ICrudAppService<
    ResPartnerDto,
    Guid,
    GetResPartnersInput,
    CreateResPartnerDto,
    UpdateResPartnerDto>
{
    Task<List<ResPartnerSelectDto>> GetSelectListAsync(string? partnerTypeCode = null);
}

