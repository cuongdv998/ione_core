using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProRuleTypes;

public interface IProRuleTypeAppService : ICrudAppService<
    ProRuleTypeDto,
    Guid,
    GetProRuleTypesInput,
    CreateProRuleTypeDto,
    UpdateProRuleTypeDto>
{
}
