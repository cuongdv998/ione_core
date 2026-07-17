using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
namespace iOne.AdminConfigs;

public class AdminConfigManager : DomainService
{
    protected IAdminConfigRepository Repository { get; }

    public AdminConfigManager(IAdminConfigRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(AdminConfig adminConfig)
    {
        // Check composite unique key (Code, SubCode)
        if (await Repository.FindByCodeSubCodeAsync(adminConfig.Code, adminConfig.SubCode) != null)
        {
            throw new BusinessException("Master:AdminConfig:CodeSubCodeExists")
                .WithData("Code", adminConfig.Code)
                .WithData("SubCode", adminConfig.SubCode);
        }

        await Repository.InsertAsync(adminConfig);
    }

    public virtual async Task UpdateAsync(
        AdminConfig adminConfig,
        string name,
        string value,
        AdminConfigStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code và subCode - Code và SubCode không được phép sửa

        adminConfig.UpdateName(name);
        adminConfig.UpdateValue(value);
        adminConfig.UpdateStatus(status);
        adminConfig.UpdateDescription(description);
        await Repository.UpdateAsync(adminConfig);
    }
}

