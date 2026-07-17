using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResRisks;

public class ResRiskManager : DomainService
{
    protected IResRiskRepository Repository { get; }

    public ResRiskManager(IResRiskRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResRisk risk)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(risk.Code))
        {
            throw new BusinessException("Master:ResRisk:CodeExists")
                .WithData("Code", risk.Code);
        }

        await Repository.InsertAsync(risk);
    }

    public virtual async Task UpdateAsync(ResRisk risk, Guid objectTypeId, string name, string? description, ResRiskStatus status)
    {
        risk.UpdateObjectTypeId(objectTypeId);
        risk.UpdateName(name);
        risk.UpdateDescription(description);
        risk.UpdateStatus(status);
        await Repository.UpdateAsync(risk);
    }
}

