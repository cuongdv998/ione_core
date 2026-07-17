using System;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResBusinessAuthorities;

public interface IResBusinessAuthorityAppService : ICrudAppService<
    ResBusinessAuthorityDto,
    Guid,
    GetResBusinessAuthoritiesInput,
    CreateResBusinessAuthorityDto,
    UpdateResBusinessAuthorityDto>
{
}
