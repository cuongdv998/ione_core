using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Workflow.WorkInstances;

public interface IWorkInstanceAppService : IApplicationService
{
    Task InitInstanceAsync(InitWorkInstanceDto input);

    Task UpdateWorkInstanceStatusAsync(UpdateWorkInstanceStatusInput input);
}
