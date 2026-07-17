using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProRules;

public class ProRuleManager : DomainService
{
    protected IProRuleRepository Repository { get; }

    public ProRuleManager(IProRuleRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProRule rule)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(rule.Code))
        {
            throw new UserFriendlyException($"Rule code '{rule.Code}' already exists");
        }

        await Repository.InsertAsync(rule);
    }

    public virtual async Task UpdateAsync(
        ProRule rule,
        string applyTo,
        Guid applyToId,
        Guid ruleTypeId,
        string name,
        string? description,
        string ruleScript,
        int priority,
        ProRuleStatus status,
        DateTime effectDate,
        DateTime? expireDate)
    {
        rule.UpdateApplyTo(applyTo);
        rule.UpdateApplyToId(applyToId);
        rule.UpdateRuleTypeId(ruleTypeId);
        rule.UpdateName(name);
        rule.UpdateDescription(description);
        rule.UpdateRuleScript(ruleScript);
        rule.UpdatePriority(priority);
        rule.UpdateStatus(status);
        rule.UpdateEffectDate(effectDate);
        rule.UpdateExpireDate(expireDate);
        await Repository.UpdateAsync(rule);
    }
}
