using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Policy.Policies;

public interface IPolicyClaimLookupAppService : IApplicationService
{
    Task<List<PolicyClaimLookupDto>> GetClaimLookupAsync(GetPolicyClaimLookupInput input);
}

