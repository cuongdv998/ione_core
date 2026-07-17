using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResIncidentCauses;

public class ResIncidentCauseManager : DomainService
{
    protected IResIncidentCauseRepository Repository { get; }

    public ResIncidentCauseManager(IResIncidentCauseRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResIncidentCause incidentCause)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(incidentCause.Code))
        {
            throw new BusinessException("Master:ResIncidentCause:CodeExists")
                .WithData("Code", incidentCause.Code);
        }

        await Repository.InsertAsync(incidentCause);
    }

    public virtual async Task UpdateAsync(ResIncidentCause incidentCause, string name, ResIncidentCauseStatus status)
    {
        incidentCause.UpdateName(name);
        incidentCause.UpdateStatus(status);
        await Repository.UpdateAsync(incidentCause);
    }
}
