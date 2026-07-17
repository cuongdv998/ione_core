using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Policy.Policies;

public interface IPolicyWorkflowStatusAppService : IApplicationService
{
    Task UpdateStatusAsync(UpdatePolicyStatusInput input);

    Task UpdateTerminationStatusAsync(UpdateTerminationStatusInput input);
}
