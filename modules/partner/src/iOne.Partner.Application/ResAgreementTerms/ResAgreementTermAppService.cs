using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Partner.Localization;
using iOne.Partner.Permissions;
using iOne.Partner.ResAgreementTerms;
using iOne.ResAgreementTerms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResAgreementTerms;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResAgreementTermPermissions.Default)]
public class ResAgreementTermAppService : CrudAppService<
    ResAgreementTerm,
    ResAgreementTermDto,
    Guid,
    GetResAgreementTermsInput,
    CreateResAgreementTermDto,
    UpdateResAgreementTermDto>,
    IResAgreementTermAppService
{
    protected ResAgreementTermManager Manager { get; }

    public ResAgreementTermAppService(
        IResAgreementTermRepository repository,
        ResAgreementTermManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(PartnerResource);
        GetPolicyName = ResAgreementTermPermissions.View;
        GetListPolicyName = ResAgreementTermPermissions.View;
        CreatePolicyName = ResAgreementTermPermissions.Create;
        UpdatePolicyName = ResAgreementTermPermissions.Edit;
        DeletePolicyName = ResAgreementTermPermissions.Delete;
    }

    public override async Task<ResAgreementTermDto> CreateAsync(CreateResAgreementTermDto input)
    {
        var entity = new ResAgreementTerm(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResAgreementTerm, ResAgreementTermDto>(entity);
    }

    public override async Task<ResAgreementTermDto> UpdateAsync(Guid id, UpdateResAgreementTermDto input)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name và Status - không update Code
        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Status
        );

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResAgreementTerm, ResAgreementTermDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResAgreementTermStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResAgreementTerm>> CreateFilteredQueryAsync(GetResAgreementTermsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by Code (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        // Filter by Name (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }
}

