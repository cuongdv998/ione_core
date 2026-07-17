using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Partner;
using iOne.Partner.Localization;
using iOne.Partner.Permissions;
using iOne.Partner.ResPartners;
using iOne.ResPartners;
using iOne.ResProvinces;
using iOne.ResWards;
using iOne.ResAgreementTerms;
using iOne.ResOrganizationTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace iOne.Partner.ResPartners;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Authorize]
public class ResPartnerAppService : CrudAppService<
    ResPartner,
    ResPartnerDto,
    Guid,
    GetResPartnersInput,
    CreateResPartnerDto,
    UpdateResPartnerDto>,
    IResPartnerAppService
{
    protected ResPartnerManager Manager { get; }
    protected IRepository<ResProvince, Guid> ProvinceRepository { get; }
    protected IRepository<ResWard, Guid> WardRepository { get; }
    protected IRepository<ResAgreementTerm, Guid> AgreementTermRepository { get; }
    protected IRepository<ResOrganizationType, Guid> OrganizationTypeRepository { get; }
    protected IRepository<ResPartnerAgreement, Guid> AgreementRepository { get; }

    public ResPartnerAppService(
        IResPartnerRepository repository,
        ResPartnerManager manager,
        IRepository<ResProvince, Guid> provinceRepository,
        IRepository<ResWard, Guid> wardRepository,
        IRepository<ResAgreementTerm, Guid> agreementTermRepository,
        IRepository<ResOrganizationType, Guid> organizationTypeRepository,
        IRepository<ResPartnerAgreement, Guid> agreementRepository)
        : base(repository)
    {
        Manager = manager;
        ProvinceRepository = provinceRepository;
        WardRepository = wardRepository;
        AgreementTermRepository = agreementTermRepository;
        OrganizationTypeRepository = organizationTypeRepository;
        AgreementRepository = agreementRepository;
        LocalizationResource = typeof(PartnerResource);
        GetPolicyName = ResPartnerPermissions.View;
        GetListPolicyName = ResPartnerPermissions.View;
        CreatePolicyName = ResPartnerPermissions.Create;
        UpdatePolicyName = ResPartnerPermissions.Edit;
        DeletePolicyName = ResPartnerPermissions.Delete;
    }

    public override async Task<ResPartnerDto> CreateAsync(CreateResPartnerDto input)
    {
        // 1. Load navigation properties để compute FullAddress
        var province = await ProvinceRepository.GetAsync(input.ProvinceId);
        var ward = await WardRepository.GetAsync(input.WardId);

        // Compute FullAddress
        var fullAddress = Manager.ComputeFullAddress(input.Address, ward.Name, province.Name);

        // Compute InvoiceFullAddress (if provided)
        string? invoiceFullAddress = null;
        if (!string.IsNullOrWhiteSpace(input.InvoiceAddress) && input.InvoiceProvinceId.HasValue && input.InvoiceWardId.HasValue)
        {
            var invoiceProvince = await ProvinceRepository.GetAsync(input.InvoiceProvinceId.Value);
            var invoiceWard = await WardRepository.GetAsync(input.InvoiceWardId.Value);
            invoiceFullAddress = Manager.ComputeFullAddress(input.InvoiceAddress, invoiceWard.Name, invoiceProvince.Name);
        }

        // 2. Validate conditional fields dựa trên OrganizationType
        if (input.OrganizationTypeId.HasValue)
        {
            var organizationType = await OrganizationTypeRepository.FindAsync(input.OrganizationTypeId.Value);

            if (organizationType != null)
            {
                ValidateConditionalFields(organizationType.Type, input);
            }
        }

        // 3. Create entity với computed FullAddress
        var entity = new ResPartner(
            GuidGenerator.Create(),
            input.PartnerTypeId,
            input.ProvinceId,
            input.WardId,
            input.Name,
            input.Address,
            fullAddress,
            input.Phone,
            input.Status,
            channelId: input.ChannelId,
            partnerRole: input.PartnerRole,
            organizationTypeId: input.OrganizationTypeId,
            code: input.Code,
            email: input.Email,
            note: input.Note,
            invoiceProvinceId: input.InvoiceProvinceId,
            invoiceWardId: input.InvoiceWardId,
            invoiceAddress: input.InvoiceAddress,
            invoiceFullAddress: invoiceFullAddress,
            idNo: input.IdNo,
            tin: input.Tin,
            repName: input.RepName,
            repEmail: input.RepEmail,
            repPhone: input.RepPhone,
            repIdNo: input.RepIdNo,
            repTitle: input.RepTitle,
            authorizer: input.Authorizer,
            authorizerPhone: input.AuthorizerPhone,
            authorizerEmail: input.AuthorizerEmail,
            authorizerNo: input.AuthorizerNo,
            authorizerDate: input.AuthorizerDate,
            authorizerTitle: input.AuthorizerTitle,
            businessNo: input.BusinessNo
        );

        await Manager.CreateAsync(entity);

        // 4. Validate và add Agreements
        if (input.Agreements != null && input.Agreements.Any())
        {
            // Remove duplicates from input (based on AgreementTermId, EffectDate, ExpireDate)
            var uniqueAgreements = input.Agreements
                .Where(a => a.AgreementTermId != Guid.Empty)
                .GroupBy(a => new
                {
                    AgreementTermId = a.AgreementTermId,
                    EffectDate = a.EffectDate,
                    ExpireDate = a.ExpireDate
                })
                .Select(g => g.First())
                .ToList();

            var newAgreements = new List<ResPartnerAgreement>();
            foreach (var agreementDto in uniqueAgreements)
            {
                var agreement = new ResPartnerAgreement(
                    GuidGenerator.Create(),
                    entity.Id,
                    agreementDto.AgreementTermId,
                    agreementDto.Value,
                    agreementDto.ExpireDate,
                    agreementDto.EffectDate
                );
                newAgreements.Add(agreement);
                entity.AddAgreement(agreement);
            }

            // Validate Agreements date ranges
            Manager.ValidateAgreementsAsync(entity.Id, newAgreements, new List<ResPartnerAgreement>());
        }

        await CurrentUnitOfWork!.SaveChangesAsync();

        return await MapToDtoWithNavigationAsync(entity);
    }

    public override async Task<ResPartnerDto> UpdateAsync(Guid id, UpdateResPartnerDto input)
    
    {
        // 1. Load entity với Agreements (bao gồm AgreementTerm navigation property)
        var query = await Repository.GetQueryableAsync();
        var entity = await query
            .Include(x => x.Channel)
            .Include(x => x.PartnerType)
            .Include(x => x.OrganizationType)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Include(x => x.InvoiceProvince)
            .Include(x => x.InvoiceWard)
            .Include(x => x.Agreements)
                .ThenInclude(a => a.AgreementTerm)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new BusinessException("Partner:ResPartner:NotFound")
                .WithData("Id", id);
        }

        // 2. Load navigation properties để compute FullAddress
        var province = await ProvinceRepository.GetAsync(input.ProvinceId);
        var ward = await WardRepository.GetAsync(input.WardId);

        // Compute FullAddress
        var fullAddress = Manager.ComputeFullAddress(input.Address, ward.Name, province.Name);

        // Compute InvoiceFullAddress (if provided)
        string? invoiceFullAddress = null;
        if (!string.IsNullOrWhiteSpace(input.InvoiceAddress) && input.InvoiceProvinceId.HasValue && input.InvoiceWardId.HasValue)
        {
            var invoiceProvince = await ProvinceRepository.GetAsync(input.InvoiceProvinceId.Value);
            var invoiceWard = await WardRepository.GetAsync(input.InvoiceWardId.Value);
            invoiceFullAddress = Manager.ComputeFullAddress(input.InvoiceAddress, invoiceWard.Name, invoiceProvince.Name);
        }

        // 3. Validate conditional fields
        if (input.OrganizationTypeId.HasValue)
        {
            var organizationType = await OrganizationTypeRepository.FindAsync(input.OrganizationTypeId.Value);

            if (organizationType != null)
            {
                ValidateConditionalFields(organizationType.Type, input);
            }
        }

        // 4. Update entity (KHÔNG update Code)
        entity.UpdateName(input.Name);
        entity.UpdateAddress(input.Address);
        entity.UpdateFullAddress(fullAddress);
        entity.UpdateEmail(input.Email);
        entity.UpdatePhone(input.Phone);
        entity.UpdateNote(input.Note);
        entity.UpdateStatus(input.Status);
        entity.UpdateChannelId(input.ChannelId);
        entity.UpdatePartnerTypeId(input.PartnerTypeId);
        entity.UpdatePartnerRole(input.PartnerRole);
        entity.UpdateOrganizationTypeId(input.OrganizationTypeId);
        entity.UpdateProvinceId(input.ProvinceId);
        entity.UpdateWardId(input.WardId);
        entity.UpdateInvoiceProvinceId(input.InvoiceProvinceId);
        entity.UpdateInvoiceWardId(input.InvoiceWardId);
        entity.UpdateInvoiceAddress(input.InvoiceAddress);
        entity.UpdateInvoiceFullAddress(invoiceFullAddress);

        // CN fields
        entity.UpdateIdNo(input.IdNo);

        // TC fields
        entity.UpdateTin(input.Tin);
        entity.UpdateRepName(input.RepName);
        entity.UpdateRepEmail(input.RepEmail);
        entity.UpdateRepPhone(input.RepPhone);
        entity.UpdateRepIdNo(input.RepIdNo);
        entity.UpdateRepTitle(input.RepTitle);
        entity.UpdateAuthorizer(input.Authorizer);
        entity.UpdateAuthorizerPhone(input.AuthorizerPhone);
        entity.UpdateAuthorizerEmail(input.AuthorizerEmail);
        entity.UpdateAuthorizerNo(input.AuthorizerNo);
        entity.UpdateAuthorizerDate(input.AuthorizerDate);
        entity.UpdateAuthorizerTitle(input.AuthorizerTitle);
        entity.UpdateBusinessNo(input.BusinessNo);

        // 5. Update Agreements (validate date ranges)
        if (input.Agreements != null)
        {
            // Lấy tất cả agreements hiện có từ database
            var existingAgreements = entity.Agreements.ToList();
            var existingAgreementIds = existingAgreements.Select(a => a.Id).ToHashSet();

            // Lọc và loại bỏ duplicates từ input (based on AgreementTermId, EffectDate, ExpireDate)
            var uniqueInputAgreements = input.Agreements
                .Where(a => a.AgreementTermId != Guid.Empty)
                .GroupBy(a => new
                {
                    AgreementTermId = a.AgreementTermId,
                    EffectDate = a.EffectDate,
                    ExpireDate = a.ExpireDate
                })
                .Select(g => g.First())
                .ToList();

            // Lấy danh sách Id từ request (chỉ những Id hợp lệ - không null và không empty)
            var inputAgreementIds = uniqueInputAgreements
                .Where(a => a.Id.HasValue && a.Id.Value != Guid.Empty)
                .Select(a => a.Id!.Value)
                .ToHashSet();

            // Xóa những agreement trong database mà không có trong request
            var agreementsToRemove = existingAgreements
                .Where(a => !inputAgreementIds.Contains(a.Id))
                .ToList();
            
            // Xóa trực tiếp từ database thông qua repository
            if (agreementsToRemove.Any())
            {
                await AgreementRepository.DeleteManyAsync(agreementsToRemove);
            }

            // Danh sách agreements để validate
            var allAgreements = new List<ResPartnerAgreement>();

            foreach (var agreementDto in uniqueInputAgreements)
            {
                if (agreementDto.Id.HasValue && agreementDto.Id.Value != Guid.Empty && existingAgreementIds.Contains(agreementDto.Id.Value))
                {
                    // Update agreement hiện có
                    var existingAgreement = existingAgreements.First(a => a.Id == agreementDto.Id.Value);
                    existingAgreement.UpdateValue(agreementDto.Value);
                    existingAgreement.UpdateEffectDate(agreementDto.EffectDate);
                    existingAgreement.UpdateExpireDate(agreementDto.ExpireDate);
                    allAgreements.Add(existingAgreement);
                }
                else
                {
                    // Tạo mới agreement (Id null hoặc empty hoặc không tồn tại trong database)
                    var newAgreement = new ResPartnerAgreement(
                        GuidGenerator.Create(),
                        entity.Id,
                        agreementDto.AgreementTermId,
                        agreementDto.Value,
                        agreementDto.ExpireDate,
                        agreementDto.EffectDate
                    );
                    allAgreements.Add(newAgreement);
                    entity.AddAgreement(newAgreement);
                }
            }

            // Validate Agreements date ranges
            Manager.ValidateAgreementsAsync(entity.Id, allAgreements, new List<ResPartnerAgreement>());
        }

        await Manager.UpdateAsync(entity);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return await MapToDtoWithNavigationAsync(entity);
    }

    public override async Task<ResPartnerDto> GetAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id, includeDetails: true);

        // Eager load navigation properties
        var query = await Repository.GetQueryableAsync();
        var entityWithNav = await query
            .Include(x => x.Channel)
            .Include(x => x.PartnerType)
            .Include(x => x.OrganizationType)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Include(x => x.InvoiceProvince)
            .Include(x => x.InvoiceWard)
            .Include(x => x.Agreements)
                .ThenInclude(a => a.AgreementTerm)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entityWithNav == null)
        {
            // This should never happen as Repository.GetAsync already throws if not found
            throw new BusinessException("Partner:ResPartner:NotFound")
                .WithData("Id", id);
        }

        return await MapToDtoWithNavigationAsync(entityWithNav);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResPartnerStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResPartner>> CreateFilteredQueryAsync(GetResPartnersInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Eager load navigation properties
        query = query
            .Include(x => x.Channel)
            .Include(x => x.PartnerType)
            .Include(x => x.OrganizationType)
            .Include(x => x.Province)
            .Include(x => x.Ward)
            .Include(x => x.InvoiceProvince)
            .Include(x => x.InvoiceWard);

        // Filter out partners whose PartnerType has been deleted (soft-delete)
        // PartnerType is required, so if navigation is null, the PartnerType was deleted
        query = query.Where(x => x.PartnerType != null);

        // Filter by Code (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code ?? "", $"%{input.Code}%"));
        }

        // Filter by Name (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
        }

        // Filter by PartnerTypeId
        if (input.PartnerTypeId.HasValue)
        {
            query = query.Where(x => x.PartnerTypeId == input.PartnerTypeId.Value);
        }

        // Filter by PartnerTypeCode (through navigation property)
        if (!string.IsNullOrWhiteSpace(input.PartnerTypeCode))
        {
            query = query.Where(x => x.PartnerType!.Code == input.PartnerTypeCode);
        }

        // Filter by OrganizationTypeId
        if (input.OrganizationTypeId.HasValue)
        {
            query = query.Where(x => x.OrganizationTypeId == input.OrganizationTypeId.Value);
        }

        // Filter by ProvinceId
        if (input.ProvinceId.HasValue)
        {
            query = query.Where(x => x.ProvinceId == input.ProvinceId.Value);
        }

        // Filter by WardId
        if (input.WardId.HasValue)
        {
            query = query.Where(x => x.WardId == input.WardId.Value);
        }

        // Filter by Status
        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        // Filter by PartnerRole
        if (!string.IsNullOrWhiteSpace(input.PartnerRole))
        {
            query = query.Where(x => x.PartnerRole == input.PartnerRole);
        }

        return query;
    }

    /// <summary>
    /// Map entity to DTO with navigation properties
    /// </summary>
    private async Task<ResPartnerDto> MapToDtoWithNavigationAsync(ResPartner entity)
    {
        var dto = ObjectMapper.Map<ResPartner, ResPartnerDto>(entity);

        // Map navigation properties
        dto.ChannelName = entity.Channel?.Name;
        dto.PartnerTypeName = entity.PartnerType?.Name ?? "";
        dto.OrganizationTypeName = entity.OrganizationType?.Name;
        dto.ProvinceName = entity.Province?.Name ?? "";
        dto.WardName = entity.Ward?.Name ?? "";
        dto.InvoiceProvinceName = entity.InvoiceProvince?.Name;
        dto.InvoiceWardName = entity.InvoiceWard?.Name ?? "";

        // Map Agreements
        if (entity.Agreements != null && entity.Agreements.Any())
        {
            dto.Agreements = entity.Agreements.Select(a => new ResPartnerAgreementDto
            {
                Id = a.Id,
                PartnerId = a.PartnerId,
                AgreementTermId = a.AgreementTermId,
                AgreementTermName = a.AgreementTerm?.Name ?? "",
                Value = a.Value,
                EffectDate = a.EffectDate,
                ExpireDate = a.ExpireDate,
                CreationTime = a.CreationTime,
                CreatorId = a.CreatorId,
                LastModificationTime = a.LastModificationTime,
                LastModifierId = a.LastModifierId
            }).ToList();
        }

        return dto;
    }

    /// <summary>
    /// Validate conditional fields dựa trên OrganizationType.Type
    /// </summary>
    private void ValidateConditionalFields(OrganizationTypeType organizationType, CreateResPartnerDto input)
    {
        if (organizationType == OrganizationTypeType.CN)
        {
            // CN (Cá nhân): Chỉ IdNo được phép, các TC fields phải null
            if (!string.IsNullOrWhiteSpace(input.Tin) ||
                !string.IsNullOrWhiteSpace(input.RepName) ||
                !string.IsNullOrWhiteSpace(input.RepEmail) ||
                !string.IsNullOrWhiteSpace(input.RepPhone) ||
                !string.IsNullOrWhiteSpace(input.RepIdNo) ||
                !string.IsNullOrWhiteSpace(input.RepTitle) ||
                !string.IsNullOrWhiteSpace(input.Authorizer) ||
                !string.IsNullOrWhiteSpace(input.AuthorizerPhone) ||
                !string.IsNullOrWhiteSpace(input.AuthorizerEmail) ||
                !string.IsNullOrWhiteSpace(input.AuthorizerNo) ||
                input.AuthorizerDate.HasValue ||
                !string.IsNullOrWhiteSpace(input.AuthorizerTitle) ||
                !string.IsNullOrWhiteSpace(input.BusinessNo))
            {
                throw new BusinessException("Partner:ResPartner:InvalidFieldsForCN")
                    .WithData("OrganizationType", "CN");
            }
        }
        else if (organizationType == OrganizationTypeType.TC)
        {
            // TC (Tổ chức): IdNo phải null
            if (!string.IsNullOrWhiteSpace(input.IdNo))
            {
                throw new BusinessException("Partner:ResPartner:InvalidFieldsForTC")
                    .WithData("OrganizationType", "TC");
            }
        }
    }

    /// <summary>
    /// Validate conditional fields dựa trên OrganizationType.Type (for Update)
    /// </summary>
    private void ValidateConditionalFields(OrganizationTypeType organizationType, UpdateResPartnerDto input)
    {
        if (organizationType == OrganizationTypeType.CN)
        {
            // CN (Cá nhân): Chỉ IdNo được phép, các TC fields phải null
            if (!string.IsNullOrWhiteSpace(input.Tin) ||
                !string.IsNullOrWhiteSpace(input.RepName) ||
                !string.IsNullOrWhiteSpace(input.RepEmail) ||
                !string.IsNullOrWhiteSpace(input.RepPhone) ||
                !string.IsNullOrWhiteSpace(input.RepIdNo) ||
                !string.IsNullOrWhiteSpace(input.RepTitle) ||
                !string.IsNullOrWhiteSpace(input.Authorizer) ||
                !string.IsNullOrWhiteSpace(input.AuthorizerPhone) ||
                !string.IsNullOrWhiteSpace(input.AuthorizerEmail) ||
                !string.IsNullOrWhiteSpace(input.AuthorizerNo) ||
                input.AuthorizerDate.HasValue ||
                !string.IsNullOrWhiteSpace(input.AuthorizerTitle) ||
                !string.IsNullOrWhiteSpace(input.BusinessNo))
            {
                throw new BusinessException("Partner:ResPartner:InvalidFieldsForCN")
                    .WithData("OrganizationType", "CN");
            }
        }
        else if (organizationType == OrganizationTypeType.TC)
        {
            // TC (Tổ chức): IdNo phải null
            if (!string.IsNullOrWhiteSpace(input.IdNo))
            {
                throw new BusinessException("Partner:ResPartner:InvalidFieldsForTC")
                    .WithData("OrganizationType", "TC");
            }
        }
    }

    [Authorize]
    public virtual async Task<List<ResPartnerSelectDto>> GetSelectListAsync(string? partnerTypeCode = null)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        
        // Include PartnerType navigation property for filtering
        query = query.Include(x => x.PartnerType);
        
        // Filter out partners whose PartnerType has been deleted (soft-delete)
        // When PartnerType is soft-deleted, the navigation property will be null due to EF Core filter
        query = query.Where(x => x.PartnerType != null);
        
        // Filter by PartnerTypeCode if provided
        if (!string.IsNullOrWhiteSpace(partnerTypeCode))
        {
            query = query.Where(x => x.PartnerType!.Code == partnerTypeCode);
        }
        
        // Sort by Name ascending
        query = query.OrderBy(x => x.Name);
        
        // Execute query and get entities
        var entities = await AsyncExecuter.ToListAsync(query);
        
        // Map entities to Select DTOs (only Id, Code, Name)
        var dtos = entities.Select(x => new ResPartnerSelectDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name
        }).ToList();
        
        return dtos;
    }
}

