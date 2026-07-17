using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProProductDistributions;

public class ProProductDistributionManager : DomainService
{
    protected IProProductDistributionRepository Repository { get; }

    public ProProductDistributionManager(IProProductDistributionRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProProductDistribution distribution)
    {
        await Repository.InsertAsync(distribution);
    }

    public virtual async Task UpdateAsync(
        ProProductDistribution distribution,
        Guid productId,
        Guid? channelId,
        Guid? appChannelId,
        Guid? employeeRoleId,
        ProProductDistributionStatus status)
    {
        distribution.UpdateProductId(productId);
        distribution.UpdateChannelId(channelId);
        distribution.UpdateAppChannelId(appChannelId);
        distribution.UpdateEmployeeRoleId(employeeRoleId);
        distribution.UpdateStatus(status);
        await Repository.UpdateAsync(distribution);
    }
}
