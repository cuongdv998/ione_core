using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResIncidentLevels;

public class ResIncidentLevelManager : DomainService
{
    protected IResIncidentLevelRepository Repository { get; }

    public ResIncidentLevelManager(IResIncidentLevelRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResIncidentLevel incidentLevel)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(incidentLevel.Code))
        {
            throw new BusinessException("Master:ResIncidentLevel:CodeExists")
                .WithData("Code", incidentLevel.Code);
        }

        await Repository.InsertAsync(incidentLevel);
    }

    public virtual async Task UpdateAsync(ResIncidentLevel incidentLevel, string name, ResIncidentLevelStatus status)
    {
        incidentLevel.UpdateName(name);
        incidentLevel.UpdateStatus(status);
        await Repository.UpdateAsync(incidentLevel);
    }
}
