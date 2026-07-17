using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProProductPlanDefinitions;

public class ProProductPlanDefinitionManager : DomainService
{
    protected IProProductPlanDefinitionRepository Repository { get; }

    public ProProductPlanDefinitionManager(IProProductPlanDefinitionRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProProductPlanDefinition planDefinition)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(planDefinition.PlanCode))
        {
            throw new BusinessException("Product:ProProductPlanDefinition:CodeExists")
                .WithData("Code", planDefinition.PlanCode);
        }

        await Repository.InsertAsync(planDefinition);
    }

    public virtual async Task UpdateAsync(
        ProProductPlanDefinition planDefinition,
        Guid productId,
        string planName,
        ProProductPlanDefinitionStatus status)
    {
        planDefinition.UpdateProductId(productId);
        planDefinition.UpdatePlanName(planName);
        planDefinition.UpdateStatus(status);
        await Repository.UpdateAsync(planDefinition);
    }
}
