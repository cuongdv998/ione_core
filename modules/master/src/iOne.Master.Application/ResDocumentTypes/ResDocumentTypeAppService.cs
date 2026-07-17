using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResDocumentTypes;
using iOne.Master.ResDocuments;
using iOne.ResDocumentTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using DocumentFormat.OpenXml.Wordprocessing;

namespace iOne.Master.ResDocumentTypes;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResDocumentTypePermissions.Default)]
public class ResDocumentTypeAppService : CrudAppService<
    ResDocumentType,
    ResDocumentTypeDto,
    Guid,
    GetResDocumentTypesInput,
    CreateResDocumentTypeDto,
    UpdateResDocumentTypeDto>,
    IResDocumentTypeAppService
{
    protected ResDocumentTypeManager Manager { get; }
    protected IMinIOService MinIOService { get; }

    public ResDocumentTypeAppService(
        IResDocumentTypeRepository repository,
        ResDocumentTypeManager manager,
        IMinIOService minIOService)
        : base(repository)
    {
        Manager = manager;
        MinIOService = minIOService;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResDocumentTypePermissions.View;
        GetListPolicyName = ResDocumentTypePermissions.View;
        CreatePolicyName = ResDocumentTypePermissions.Create;
        UpdatePolicyName = ResDocumentTypePermissions.Edit;
        DeletePolicyName = ResDocumentTypePermissions.Delete;
    }

    public override async Task<ResDocumentTypeDto> CreateAsync(CreateResDocumentTypeDto input)
    {
        // Bucket is required, validate and normalize
        var bucketName = input.Bucket.Trim().ToLowerInvariant();

        // Validate MinIO bucket name format
        if (!IsValidMinIOBucketName(bucketName))
        {
            throw new UserFriendlyException(
                L["ResDocumentType:BucketInvalid"].Value
            );
        }

        // Create bucket in MinIO
        await MinIOService.CreateBucketAsync(bucketName);

        var entity = new ResDocumentType(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status,
            input.Description,
            bucketName,
            input.DocumentGroupCode
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResDocumentType, ResDocumentTypeDto>(entity);
    }

    /// <summary>
    /// Validates MinIO bucket name format
    /// MinIO bucket name rules:
    /// - 3-63 characters
    /// - Lowercase letters, numbers, dots (.), and hyphens (-) only
    /// - Cannot start or end with dot or hyphen
    /// - Cannot contain consecutive dots
    /// - Cannot be an IP address format
    /// </summary>
    private bool IsValidMinIOBucketName(string bucketName)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            return false;
        }

        // Length check: 3-63 characters
        if (bucketName.Length < 3 || bucketName.Length > 63)
        {
            return false;
        }

        // Must be lowercase
        if (bucketName != bucketName.ToLowerInvariant())
        {
            return false;
        }

        // Cannot start or end with dot or hyphen
        if (bucketName[0] == '.' || bucketName[0] == '-' ||
            bucketName[bucketName.Length - 1] == '.' || bucketName[bucketName.Length - 1] == '-')
        {
            return false;
        }

        // Cannot contain consecutive dots
        if (bucketName.Contains(".."))
        {
            return false;
        }

        // Can only contain lowercase letters, numbers, dots, and hyphens
        if (!Regex.IsMatch(bucketName, @"^[a-z0-9][a-z0-9\-\.]{1,61}[a-z0-9]$"))
        {
            return false;
        }

        // Cannot be an IP address format (e.g., 192.168.1.1)
        if (Regex.IsMatch(bucketName, @"^\d+\.\d+\.\d+\.\d+$"))
        {
            return false;
        }

        return true;
    }

    public override async Task<ResDocumentTypeDto> UpdateAsync(Guid id, UpdateResDocumentTypeDto input)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description, Status - không update Code và Bucket
        // Giữ nguyên giá trị bucket hiện tại (không cho phép sửa)
        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Status,
            input.Description,
            entity.Bucket, // Giữ nguyên bucket hiện tại, không cho phép sửa
            input.DocumentGroupCode
        );

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResDocumentType, ResDocumentTypeDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResDocumentTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResDocumentType>> CreateFilteredQueryAsync(GetResDocumentTypesInput input)
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

    public async Task<ResDocumentTypeDto> GetByCodeAsync(string code)
    {
        var docType = await Manager.GetByCode(code);

        return ObjectMapper.Map<ResDocumentType, ResDocumentTypeDto>(docType);
    }

    public async Task<List<ResDocumentTypeDto>> GetListByDocumentGroupCodeAsync(string documentGroupCode)
    {
        if (string.IsNullOrWhiteSpace(documentGroupCode))
        {
            throw new UserFriendlyException("Document group code is required.");
        }

        var documentTypes = await Manager.GetListByDocumentGroupCodeAsync(documentGroupCode);
        return ObjectMapper.Map<List<ResDocumentType>, List<ResDocumentTypeDto>>(documentTypes);
    }
}
