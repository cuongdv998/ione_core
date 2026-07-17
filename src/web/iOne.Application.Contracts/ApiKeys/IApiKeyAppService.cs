using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.ApiKeys;

public interface IApiKeyAppService : IApplicationService
{
    Task<CreateApiKeyResultDto> CreateAsync(CreateApiKeyDto input);

    Task<List<ApiKeyDto>> GetListAsync();

    Task RevokeAsync(Guid id);
}
