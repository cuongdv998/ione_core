using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Policy.PolicyContracts;

public interface IPolicyContractAppService : ICrudAppService<
    PolicyContractDto,
    Guid,
    GetPolicyContractsInput,
    CreatePolicyContractDto,
    UpdatePolicyContractDto>
{
    Task<PolicyContractDto> CreateWithResultAsync(CreatePolicyContractDto input);
    Task<PagedResultDto<PolicyContractSearchResultDto>> SearchAsync(PolicyContractSearchInput input);
    Task CancelAsync(Guid id);
    Task<byte[]> ExportAsync(PolicyContractSearchInput input);
    Task<byte[]> ExportExcelAsync(PolicyContractSearchInput input);

    /// <summary>
    /// Terminates a contract and all its active policies.
    /// </summary>
    Task TerminateContractAsync(TerminateContractInput input);
}
