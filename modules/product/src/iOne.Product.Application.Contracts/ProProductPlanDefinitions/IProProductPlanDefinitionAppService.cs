using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProProductPlanDefinitions;

public interface IProProductPlanDefinitionAppService : ICrudAppService<
    ProProductPlanDefinitionDto,
    Guid,
    GetProProductPlanDefinitionsInput,
    CreateProProductPlanDefinitionDto,
    UpdateProProductPlanDefinitionDto>
{
    Task<List<ProProductPlanDefinitionDto>> GetByProductIdAsync(Guid productId);
}
