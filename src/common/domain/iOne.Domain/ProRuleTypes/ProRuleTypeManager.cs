using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProRuleTypes;

public class ProRuleTypeManager : DomainService
{
    protected IProRuleTypeRepository Repository { get; }

    public ProRuleTypeManager(IProRuleTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProRuleType ruleType)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(ruleType.Code))
        {
            throw new BusinessException("Product:ProRuleType:CodeExists")
                .WithData("Code", ruleType.Code);
        }

        await Repository.InsertAsync(ruleType);
    }

    public virtual async Task UpdateAsync(ProRuleType ruleType, string name, string? description, ProRuleTypeStatus status)
    {
        ruleType.UpdateName(name);
        ruleType.UpdateDescription(description);
        ruleType.UpdateStatus(status);
        await Repository.UpdateAsync(ruleType);
    }
}
