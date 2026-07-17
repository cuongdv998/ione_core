using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProRules;

public interface IProRuleAppService : ICrudAppService<
    ProRuleDto,
    Guid,
    GetProRulesInput,
    CreateProRuleDto,
    UpdateProRuleDto>
{
}
