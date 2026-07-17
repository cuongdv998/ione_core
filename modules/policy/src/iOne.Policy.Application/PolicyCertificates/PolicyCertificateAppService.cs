using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Policy;
using iOne.Policy.Localization;
using iOne.Policy.Permissions;
using iOne.Policy.PolicyCertificates;
using iOne.Policies; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Policy.PolicyCertificates;

[RemoteService(Name = PolicyRemoteServiceConsts.RemoteServiceName)]
[Authorize(PolicyCertificatePermissions.Default)]
public class PolicyCertificateAppService : CrudAppService<
    PolicyCertificate,
    PolicyCertificateDto,
    Guid,
    GetPolicyCertificatesInput,
    CreatePolicyCertificateDto,
    UpdatePolicyCertificateDto>, IPolicyCertificateAppService
{
    protected PolicyCertificateManager Manager { get; }
    protected IPolicyCertificateRepository PolicyCertificateRepository { get; }

    public PolicyCertificateAppService(
        IRepository<PolicyCertificate, Guid> repository,
        PolicyCertificateManager manager,
        IPolicyCertificateRepository policyCertificateRepository)
        : base(repository)
    {
        Manager = manager;
        PolicyCertificateRepository = policyCertificateRepository;
        LocalizationResource = typeof(PolicyResource);
        GetPolicyName = PolicyCertificatePermissions.View;
        GetListPolicyName = PolicyCertificatePermissions.View;
        CreatePolicyName = PolicyCertificatePermissions.Create;
        UpdatePolicyName = PolicyCertificatePermissions.Edit;
        DeletePolicyName = PolicyCertificatePermissions.Delete;
    }

    public virtual async Task<PolicyCertificateDto> GetUrlByPolicyIdAsync(Guid policyId, Guid? policyVersionId = null)
    {
        var queryable = await ReadOnlyRepository.GetQueryableAsync();
        var query = queryable.Where(x => x.PolicyId == policyId);

        if (policyVersionId.HasValue)
        {
            query = query.Where(x => x.PolicyVersionId == policyVersionId.Value);
        }

        var certificate = await query
            .OrderByDescending(x => x.CreationTime)
            .FirstOrDefaultAsync();

        return ObjectMapper.Map<PolicyCertificate, PolicyCertificateDto>(certificate);
    }

    public override async Task<PolicyCertificateDto> CreateAsync(CreatePolicyCertificateDto input)
    {
        var entity = new PolicyCertificate(
            GuidGenerator.Create(),
            input.PolicyId,
            input.PolicyVersionId,
            input.CertificateNo,
            input.Url
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<PolicyCertificate, PolicyCertificateDto>(entity);
    }

    public override async Task<PolicyCertificateDto> UpdateAsync(Guid id, UpdatePolicyCertificateDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // Update entity properties
        entity.UpdateCertificateNo(input.CertificateNo);
        entity.UpdateUrl(input.Url);

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<PolicyCertificate, PolicyCertificateDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<PolicyCertificate>> CreateFilteredQueryAsync(GetPolicyCertificatesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by PolicyId
        if (input.PolicyId.HasValue)
        {
            query = query.Where(x => x.PolicyId == input.PolicyId.Value);
        }

        // Filter by PolicyVersionId
        if (input.PolicyVersionId.HasValue)
        {
            query = query.Where(x => x.PolicyVersionId == input.PolicyVersionId.Value);
        }

        // Filter by CertificateNo (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.CertificateNo))
        {
            query = query.Where(x => EF.Functions.ILike(x.CertificateNo ?? "", $"%{input.CertificateNo}%"));
        }

        return query;
    }
}
